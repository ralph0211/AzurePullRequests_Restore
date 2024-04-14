using AzurePullRequests.Restore.Interfaces;
using AzurePullRequests.Restore.Services;
using AzurePullRequests.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzurePullRequests.Restore
{
    public class Program
    {
        static async Task Main()
        {
            var serviceProvider = ConfigureServices();

            var azureDevOpsRestoreService = serviceProvider.GetRequiredService<IAzureDevOpsRestoreService>();

            await azureDevOpsRestoreService.RestorePullRequestsAsync();
        }

        static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(provider =>
                new AppSettings());

            services.AddScoped<IAzureDevOpsRestoreService, AzureDevOpsRestoreService>();

            return services.BuildServiceProvider();
        }
    }
}
