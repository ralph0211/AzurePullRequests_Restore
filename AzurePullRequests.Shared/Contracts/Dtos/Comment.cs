namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class Comment
    {
        public int Id { get; set; }

        public IdentityRef Author { get; set; }

        public string Content { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime LastContentUpdatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public int ParentCommentId { get; set; }

        public DateTime PublishedDate { get; set; }

        public List<IdentityRef> UsersLiked { get; set; }
    }
}
