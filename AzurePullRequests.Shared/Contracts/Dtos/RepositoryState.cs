namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class RepositoryState
    {
        public List<PullRequestState> PullRequests { get; set; }

        public string BackupLocation { get; set; }
    }
}
