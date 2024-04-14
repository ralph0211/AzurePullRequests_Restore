namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class GitPullRequestCommentThread
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public DateTime PublishedDate { get; set; }

        public List<Comment> Comments { get; set; }
    }
}
