namespace AzurePullRequests.Restore.Interfaces
{
    public interface IAzureDevOpsRestoreService
    {
        Task RestorePullRequestsAsync();
    }
}
