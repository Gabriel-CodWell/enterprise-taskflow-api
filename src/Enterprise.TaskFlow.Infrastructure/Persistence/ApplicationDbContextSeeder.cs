using Enterprise.TaskFlow.Domain.Entities;
using Enterprise.TaskFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Enterprise.TaskFlow.Infrastructure.Persistence;

public static class ApplicationDbContextSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            if (await context.TaskItems.AnyAsync())
            {
                logger.LogInformation("Database already seeded with TaskItems.");
                return;
            }

            logger.LogInformation("Seeding default TaskItems...");

            var tasks = new List<TaskItem>
            {
                new TaskItem 
                { 
                    Title = "Configure CI/CD Pipeline", 
                    Description = "Set up GitHub Actions for automated build and tests.",
                    Status = TaskItemStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                },
                new TaskItem 
                { 
                    Title = "Implement Authentication", 
                    Description = "Add JWT Bearer authentication to the API.",
                    Status = TaskItemStatus.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new TaskItem 
                { 
                    Title = "Write Unit Tests", 
                    Description = "Achieve 80% code coverage for the Application layer.",
                    Status = TaskItemStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                }
            };

            await context.TaskItems.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Successfully seeded {Count} TaskItems.", tasks.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
