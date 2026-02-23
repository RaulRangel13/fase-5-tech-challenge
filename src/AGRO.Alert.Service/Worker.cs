using System.Text;
using System.Text.Json;
using AGRO.Alert.Service.Domain.Entities;
using AGRO.Alert.Service.Infrastructure.Data;
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
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
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

        // Regra Simples: Umidade < 30%
        if (data.SoilMoisture < 30)
        {
            _logger.LogWarning("ALERTA DE SECA DETECTADO! Umidade: {Moisture}%", data.SoilMoisture);

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
                var alert = new AgroAlert
                {
                    FieldId = data.FieldId,
                    Message = "Risco de seca detectado. Umidade do solo abaixo de 30%.",
                    Severity = "Warning",
                    TriggerValue = data.SoilMoisture,
                    Timestamp = DateTime.UtcNow
                };
                db.Alerts.Add(alert);
                await db.SaveChangesAsync();
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
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Rainfall { get; set; }
}
