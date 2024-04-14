using AzurePullRequests.Restore.Interfaces;
using AzurePullRequests.Shared.Configuration;
using AzurePullRequests.Shared.Contracts.Dtos;

namespace AzurePullRequests.Restore.Services
{
    public class AzureDevOpsRestoreService : IAzureDevOpsRestoreService
    {
        private readonly AppSettings _appSettings;
        private readonly string _patToken;
        private readonly string _organizationUrl;
        private readonly string _backupFolder;

        public AzureDevOpsRestoreService(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _patToken = $"{_appSettings.AzureDevOpsAuth.PatToken}";
            _organizationUrl = $"{_appSettings.AzureDevOpsAuth.BaseUrl}{_appSettings.OrganizationName}";
            _backupFolder = $"C:/{_appSettings.ProjectName}/backup";
        }

        public async Task RestorePullRequestsAsync()
        {
            var restoreStateFile = _appSettings.RestoreStateFile;

            if (string.IsNullOrEmpty(restoreStateFile))
            {
                Console.WriteLine("RestoreStateFile parameter not set in appsettings!");
                throw new ArgumentNullException(nameof(restoreStateFile), "RestoreStateFile parameter not set in appsettings!");
            }

            var filePath = Path.Combine(_backupFolder, restoreStateFile);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} not found. Please enter valid file name!");
                throw new FileNotFoundException("File not found in backup folder", filePath);
            }
        }
    }
}
