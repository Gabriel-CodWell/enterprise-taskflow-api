using Enterprise.TaskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enterprise.TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for <see cref="TaskItem"/>.
/// Configures optimistic concurrency via a SQL Server <c>rowversion</c> column.
/// </summary>
public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Description)
            .HasMaxLength(2048);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Optimistic concurrency — maps to SQL Server's rowversion/timestamp column
        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // Index on Status for the "get pending tasks" query
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_TaskItems_Status");
    }
}
