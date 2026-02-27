using System.Text;
using System.Text.Json;
using AGRO.Alert.Service.Domain.Entities;
using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AGRO.Alert.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        InitializeRabbitMQ();
        return base.StartAsync(cancellationToken);
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"]
                ?? throw new InvalidOperationException("RabbitMQ:Password não configurado. Defina via variável de ambiente (veja docs/SECRETS.md).")
        };
        
        // Retry logic for connection
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (ex, time) => _logger.LogWarning(ex, "Could not connect to RabbitMQ, retrying..."));

        policy.Execute(() => {
             _connection = factory.CreateConnection();
             _channel = _connection.CreateModel();
             _channel.QueueDeclare(queue: "telemetry_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation($"Received message: {content}");

            try
            {
                var data = JsonSerializer.Deserialize<TelemetryDto>(content);
                await ProcessAlertRules(data);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume("telemetry_queue", false, consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessAlertRules(TelemetryDto? data)
    {
        if (data == null) return;

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();

        // 1. Persistir dado do sensor para histórico e gráficos
        var reading = new SensorReading
        {
            FieldId = data.FieldId,
            Timestamp = data.Timestamp,
            SoilMoisture = data.SoilMoisture,
            Temperature = data.Temperature,
            Rainfall = data.Rainfall
        };
        db.SensorReadings.Add(reading);
        await db.SaveChangesAsync();

        // 2. Regra de Alerta de Seca: umidade < 30% por mais de 24 horas
        if (data.SoilMoisture < 30)
        {
            var twentyFourHoursAgo = data.Timestamp.AddHours(-24);
            var readingsLast24h = await db.SensorReadings
                .Where(r => r.FieldId == data.FieldId && r.Timestamp >= twentyFourHoursAgo)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();

            var allBelow30 = readingsLast24h.All(r => r.SoilMoisture < 30);
            var spans24h = readingsLast24h.Count >= 2 &&
                (readingsLast24h[^1].Timestamp - readingsLast24h[0].Timestamp).TotalHours >= 24;

            // Não criar alerta duplicado: verificar se já existe alerta de seca nos últimos 24h
            var lastDroughtAlert = await db.Alerts
                .Where(a => a.FieldId == data.FieldId && a.Message.Contains("seca"))
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync();
            var alreadyAlerted = lastDroughtAlert != null &&
                (data.Timestamp - lastDroughtAlert.Timestamp).TotalHours < 24;

            if (allBelow30 && spans24h && !alreadyAlerted)
            {
                _logger.LogWarning("ALERTA DE SECA! Umidade < 30% por 24h no talhão {FieldId}", data.FieldId);
                db.Alerts.Add(new AgroAlert
                {
                    FieldId = data.FieldId,
                    Message = "Alerta de Seca: Umidade do solo abaixo de 30% por mais de 24 horas.",
                    Severity = "Warning",
                    TriggerValue = data.SoilMoisture,
                    Timestamp = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }

        // 3. Regra de Risco de Praga: temperatura > 38°C por período prolongado
        if (data.Temperature > 38)
        {
            var sixHoursAgo = data.Timestamp.AddHours(-6);
            var recentHighTemp = await db.SensorReadings
                .Where(r => r.FieldId == data.FieldId && r.Timestamp >= sixHoursAgo && r.Temperature > 38)
                .CountAsync();

            if (recentHighTemp >= 3) // 3+ leituras com temp alta em 6h
            {
                var lastPestAlert = await db.Alerts
                    .Where(a => a.FieldId == data.FieldId && a.Message.Contains("Praga"))
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefaultAsync();
                if (lastPestAlert == null || (data.Timestamp - lastPestAlert.Timestamp).TotalHours >= 6)
                {
                    _logger.LogWarning("RISCO DE PRAGA! Temperatura elevada no talhão {FieldId}", data.FieldId);
                    db.Alerts.Add(new AgroAlert
                    {
                        FieldId = data.FieldId,
                        Message = "Risco de Praga: Temperatura elevada por período prolongado.",
                        Severity = "Warning",
                        TriggerValue = data.Temperature,
                        Timestamp = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

// Helper class for Retry
public static class Policy {
    public static PolicyBuilder Handle<T>() where T : Exception => new PolicyBuilder();
}
public class PolicyBuilder {

    public SimplePolicy WaitAndRetry(int count, Func<int, TimeSpan> sleepProvider, Action<Exception, TimeSpan> onRetry) {
         return new SimplePolicy(count, sleepProvider, onRetry);
    }
}
public class SimplePolicy {
    private int _count;
    private Func<int, TimeSpan> _sleepProvider;
    private Action<Exception, TimeSpan> _onRetry;
    public SimplePolicy(int c, Func<int, TimeSpan> s, Action<Exception, TimeSpan> o) { _count=c; _sleepProvider=s; _onRetry=o; }
    public void Execute(Action action) {
        for(int i=0; i<_count; i++) {
            try { action(); return; }
            catch(Exception ex) {
                if (i == _count - 1) throw;
                var sleep = _sleepProvider(i+1);
                _onRetry(ex, sleep);
                Thread.Sleep(sleep);
            }
        }
    }
}

public class TelemetryDto
{
    public Guid FieldId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Rainfall { get; set; }
}
