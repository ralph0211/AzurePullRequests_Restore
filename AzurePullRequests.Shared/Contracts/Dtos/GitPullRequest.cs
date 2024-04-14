namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class GitPullRequest
    {
        public int Id { get; set; }

        public string ArtifactId { get; set; }

        public Repository Repository { get; set; }

        public int PullRequestId { get; set; }

        public int CodeReviewId { get; set; }

        public string Status { get; set; }

        public IdentityRef CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string SourceRefName { get; set; }

        public string TargetRefName { get; set; }

        public string MergeStatus { get; set; }

        public Guid MergeId { get; set; }

        public GitCommitRef LastMergeSourceCommit { get; set; }

        public GitCommitRef LastMergeTargetCommit { get; set; }

        public GitCommitRef LastMergeCommit { get; set; }

        public IdentityRefWithVote[] Reviewers { get; set; }

        public string Url { get; set; }

        public bool SupportsIterations { get; set; }

        // PLUS MANY MORE...
    }
}
