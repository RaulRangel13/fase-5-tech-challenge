using System.Text;
using System.Text.Json;
using AGRO.Ingestion.Service.Application.DTOs;
using RabbitMQ.Client;

namespace AGRO.Ingestion.Service.Infrastructure.Messaging;

public class RabbitMqPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        _factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };
    }

    public void Publish(TelemetryData data)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "telemetry_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var json = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: "",
                             routingKey: "telemetry_queue",
                             basicProperties: properties,
                             body: body);
    }
}
