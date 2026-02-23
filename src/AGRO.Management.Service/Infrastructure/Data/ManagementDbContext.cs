using AGRO.Management.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGRO.Management.Service.Infrastructure.Data;

public class ManagementDbContext : DbContext
{
    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
    }

    public DbSet<Farm> Farms { get; set; }
    public DbSet<Field> Fields { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Farm>()
            .HasMany(f => f.Fields)
            .WithOne(f => f.Farm)
            .HasForeignKey(f => f.FarmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
