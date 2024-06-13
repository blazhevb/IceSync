using IceSync.Application.Managers;
using IceSync.Domain.Contracts;
using IceSync.Domain.Contracts.Managers;
using IceSync.Infrastructure.ApiClients;
using IceSync.Infrastructure.Configurations;
using IceSync.Infrastructure.Contracts;
using IceSync.Infrastructure.Handlers;
using IceSync.Infrastructure.Mappers;
using IceSync.Infrastructure.Services;
using IceSync.Persistence.Data;
using IceSync.Persistence.Repositories;
using IceSync.Workers.Configurations;
using IceSync.Workers.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;

namespace IceSync.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .CreateLogger();

            services.AddSerilog(Log.Logger);

            services.AddMemoryCache();

            services.Configure<CredentialsOptions>(configuration.GetSection("CredentialsOptions"));
            services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));
            services.Configure<SynchronizationWorkerOptions>(configuration.GetSection("SynchronizationWorkerOptions"));

            services.AddDbContext<IceSyncDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<AuthenticationHandler>();

            services.AddHttpClient("UniversalLoaderAPIClient")
               .AddHttpMessageHandler<AuthenticationHandler>();

            services.AddTransient(sp =>
            {
                IHttpClientFactory factory = sp.GetRequiredService<IHttpClientFactory>();
                HttpClient client = factory.CreateClient("UniversalLoaderAPIClient");
                IOptions<ApiOptions> apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>();

                UniversalLoaderAPIClient universalLoaderAPIClient = new(apiOptions.Value.BaseUrl, client);

                return universalLoaderAPIClient;
            });

            services.AddTransient(sp =>
            {
                IOptions<ApiOptions> apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>();
                UniversalLoaderAPIClient apiClient = new(apiOptions.Value.BaseUrl, new HttpClient());

                var credentialsOptions = sp.GetRequiredService<IOptions<CredentialsOptions>>();
                var logger = sp.GetRequiredService<ILogger<UniversalLoaderTokenProvider>>();
                var memoryCache = sp.GetRequiredService<IMemoryCache>();

                return new UniversalLoaderTokenProvider(apiClient, credentialsOptions, logger, memoryCache);
            });

            services.AddSingleton<IWorkflowMapper, WorkFlowMapper>();

            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IExternalWorkflowService, UniversalLoaderWorkflowService>();
            services.AddScoped<IWorkflowManager, WorkflowManager>();

            services.AddHostedService<SynchronizationService>();
        }
    }
}
