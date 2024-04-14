namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class IdentityRefWithVote: IdentityRef
    {
        public int Vote { get; set; }

        public string ReviewerUrl { get; set; }
    }
}
