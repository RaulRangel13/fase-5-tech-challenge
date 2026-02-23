using AGRO.Alert.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Alert.Service.Infrastructure.Data;

public class AlertDbContext : DbContext
{
    public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options)
    {
    }

    public DbSet<AgroAlert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
