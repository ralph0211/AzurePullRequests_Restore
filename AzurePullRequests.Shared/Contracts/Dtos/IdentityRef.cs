namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class IdentityRef
    {
        public Guid Id { get; set; }

        public string DisplayName { get; set; }

        public string UniqueName { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }
    }
}
