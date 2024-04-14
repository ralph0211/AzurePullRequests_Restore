namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class PullRequestCommentThreadResponse
    {
        public List<GitPullRequestCommentThread> Value { get; set; }

        public int Count { get; set; }
    }
}
