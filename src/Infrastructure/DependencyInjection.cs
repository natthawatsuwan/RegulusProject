using RegulusProject.Application.Common.Interfaces;
using RegulusProject.Domain.Constants;
using RegulusProject.Infrastructure.Data;
using RegulusProject.Infrastructure.Data.Interceptors;
using RegulusProject.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.PostgreSql;
using Serilog.Core;
using Serilog;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");
        var SeqServerUrl = Environment.GetEnvironmentVariable("SeqServerUrl") ?? configuration["Seq:SeqServerUrl"];
        Guard.Against.Null(SeqServerUrl, message: "SeqServerUrl' not found.");

        var levelSwitch = new LoggingLevelSwitch();
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.Seq(SeqServerUrl, controlLevelSwitch: levelSwitch)
            .CreateLogger();

        Serilog.Log.Information("Starting up");


        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        var HangfireConnectionString = Environment.GetEnvironmentVariable("HangfireConnection") ?? configuration.GetConnectionString("HangfireConnection");
        Guard.Against.Null(HangfireConnectionString, message: "Connection string 'HangfireConnectionString' not found.");

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSeq(Environment.GetEnvironmentVariable("SeqServerUrl") ?? configuration["Seq:SeqServerUrl"]);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Environment.GetEnvironmentVariable("RedisConfiguration") ?? configuration["Redis:RedisConfiguration"];
            //options.InstanceName = "RedisInstance";
            //options.Configuration = "localhost:6379";
            options.InstanceName = "RedisInstance";
        });
        //localhost:6379
        var redisConnectionString = Environment.GetEnvironmentVariable("RedisConfiguration") ?? configuration["Redis:RedisConfiguration"];
        //var redisConnectionString = "localhost:6379";
        Guard.Against.Null(redisConnectionString, message: "redisConnectionString' not found.");
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        //services.AddScoped<IRedisCacheService, RedisCacheService>();

        //Create file attachment directory 
        string? fileAttachmentDirectory = configuration["FileSettings:AttachmentDirectory"];
        if (fileAttachmentDirectory != null && !Directory.Exists(fileAttachmentDirectory))
        {
            Directory.CreateDirectory(fileAttachmentDirectory);
        }


        services.AddHangfire(configuration =>
        {
            configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            configuration.UseSimpleAssemblyNameTypeSerializer();
            configuration.UseRecommendedSerializerSettings();
            configuration.UsePostgreSqlStorage(c =>
            {
                c.UseNpgsqlConnection(HangfireConnectionString);
            });
        }).AddHangfireServer();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddSingleton(TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return services;
    }
}
