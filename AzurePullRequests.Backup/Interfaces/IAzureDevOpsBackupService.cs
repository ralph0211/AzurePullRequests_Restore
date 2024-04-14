using AzurePullRequests.Shared.Contracts.Dtos;

namespace AzurePullRequests.Backup.Interfaces
{
    public interface IAzureDevOpsBackupService
    {
        Task<PullRequestsResponse> BackupActivePullRequestsAsync();
    }
}