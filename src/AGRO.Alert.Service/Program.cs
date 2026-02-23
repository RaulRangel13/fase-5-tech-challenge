using AGRO.Alert.Service;
using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

// DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Host=localhost;Port=5432;Database=agro_db;Username=admin;Password=adminpassword";

builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<Worker>();

// Metrics server for Worker (standalone Kestrel for metrics)
// Since it's a worker, we need to explicitly start a metric server
var metricsServer = new KestrelMetricServer(port: 8080);
metricsServer.Start();

var host = builder.Build();

// Ensure DB Created
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
    db.Database.EnsureCreated();
}

host.Run();
