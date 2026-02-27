using AGRO.Alert.Service;
using AGRO.Alert.Service.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// DB Context - valores devem vir de variáveis de ambiente
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Defina ConnectionStrings__DefaultConnection (ex.: via .env ou variável de ambiente). Veja .env.example e docs/SECRETS.md.");

builder.Services.AddDbContext<AlertDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgroSolutions - Serviço de Alertas",
        Version = "v1",
        Description =
            "API para consultar alertas por talhão, histórico de telemetria (para gráficos) e status geral do talhão.\n\n" +
            "Regras implementadas (MVP):\n" +
            "- Alerta de Seca: umidade do solo < 30% por mais de 24 horas.\n" +
            "- Risco de Praga: temperatura elevada por período prolongado."
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.MapControllers();
app.MapMetrics();

app.Run();
