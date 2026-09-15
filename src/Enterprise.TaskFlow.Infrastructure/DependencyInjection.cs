using Enterprise.TaskFlow.Domain.Interfaces;
using Enterprise.TaskFlow.Infrastructure.Caching;
using Enterprise.TaskFlow.Infrastructure.Messaging;
using Enterprise.TaskFlow.Infrastructure.Persistence;
using Enterprise.TaskFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Enterprise.TaskFlow.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services (EF Core, Redis, RabbitMQ) into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core + SQL Server ──────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // ── Redis ─────────────────────────────────────────────────────
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 3000;
                return ConnectionMultiplexer.Connect(options);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis is not available at '{ConnectionString}'. Cache operations will fail gracefully.", redisConnectionString);
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 1000;
                return ConnectionMultiplexer.Connect(options);
            }
        });

        // ── Repositories & Services ───────────────────────────────────
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IEventPublisher>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqEventPublisher>>();
            try
            {
                return new RabbitMqEventPublisher(configuration, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ is not available. Event publishing will use a no-op fallback.");
                return new NoOpEventPublisher(logger);
            }
        });

        return services;
    }
}

