using AzurePullRequests.Shared.Configuration;
using AzurePullRequests.Backup.Services;
using Microsoft.Extensions.DependencyInjection;
using AzurePullRequests.Backup.Interfaces;

namespace AzurePullRequests.Backup
{
    public class Program
    {
        static async Task Main()
        {
            var serviceProvider = ConfigureServices();

            var azureDevOpsBackupService = serviceProvider.GetRequiredService<IAzureDevOpsBackupService>();

            await azureDevOpsBackupService.BackupActivePullRequestsAsync();
        }

        static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(provider =>
                new AppSettings());

            services.AddScoped<IAzureDevOpsBackupService, AzureDevOpsBackupService>();

            return services.BuildServiceProvider();
        }
    }
}
