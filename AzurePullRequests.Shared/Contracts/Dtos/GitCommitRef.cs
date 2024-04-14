using AzurePullRequests.Shared.Contracts.Dtos;

namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class GitCommitRef
    {
        public string CommitId { get; set; }

        public string Url { get; set; }

        public GitUserDate Author { get; set; }
    }
}
