using Xunit;
using AGRO.Alert.Service.Controllers;
using AGRO.Alert.Service.Infrastructure.Data;
using AGRO.Alert.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Tests;

public class AlertServiceTests
{
    private static AlertDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AlertDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AlertDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetAlerts_WithoutFilter_ReturnsEmptyList()
    {
        var db = GetInMemoryDbContext();
        var controller = new AlertsController(db);

        var result = await controller.GetAlerts(null);

        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<AgroAlert>>(ok.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAlerts_WithFieldId_ReturnsOnlyThatFieldAlerts()
    {
        var db = GetInMemoryDbContext();
        var fieldId = Guid.NewGuid();
        db.Alerts.Add(new AgroAlert
        {
            FieldId = fieldId,
            Message = "Alerta de seca",
            Severity = "Warning",
            TriggerValue = 25
        });
        await db.SaveChangesAsync();
        var controller = new AlertsController(db);

        var result = await controller.GetAlerts(fieldId);

        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<AgroAlert>>(ok.Value);
        Assert.Single(list);
        Assert.Equal(fieldId, list.First().FieldId);
    }

    [Fact]
    public async Task GetFieldStatus_WhenNoData_ReturnsNormal()
    {
        var db = GetInMemoryDbContext();
        var controller = new StatusController(db);
        var fieldId = Guid.NewGuid();

        var result = await controller.GetFieldStatus(fieldId);

        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var value = ok.Value;
        var statusProp = value?.GetType().GetProperty("status");
        Assert.NotNull(statusProp);
        Assert.Equal("Normal", statusProp.GetValue(value)?.ToString());
    }

    [Fact]
    public async Task GetReadings_ReturnsEmptyWhenNoData()
    {
        var db = GetInMemoryDbContext();
        var controller = new TelemetryController(db);
        var fieldId = Guid.NewGuid();

        var result = await controller.GetReadings(fieldId, null, null, 100);

        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
        Assert.Empty(list);
    }
}
