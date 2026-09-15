using Enterprise.TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.TaskFlow.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Enterprise TaskFlow application.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
