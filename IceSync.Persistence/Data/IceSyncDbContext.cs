using IceSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Persistence.Data;

public class IceSyncDbContext : DbContext
{
    public IceSyncDbContext(DbContextOptions<IceSyncDbContext> options) : base(options)
    {
    }

    public DbSet<Workflow> Workflows { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.HasKey(e => e.WorkflowID);
            entity.Property(e => e.WorkflowID).ValueGeneratedNever();
            entity.Property(e => e.WorkflowName).IsRequired(false);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.MultiExecBehavior).IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}