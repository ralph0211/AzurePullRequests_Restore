namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class PullRequestState
    {
        public int Id { get; set; }

        public DateTime LastMergeCommit { get; set; }

        public DateTime ThreadLastUpdated { get; set; }
    }
}
