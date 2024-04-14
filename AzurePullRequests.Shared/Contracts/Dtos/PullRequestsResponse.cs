namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class PullRequestsResponse
    {
        public List<GitPullRequest> Value { get; set; }

        public int Count { get; set; }
    }
}
