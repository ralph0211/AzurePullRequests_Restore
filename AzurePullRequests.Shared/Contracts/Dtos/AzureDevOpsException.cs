namespace AzurePullRequests.Shared.Contracts.Dtos
{
    public class AzureDevOpsException : Exception
    {
        public AzureDevOpsException(string message)
            : base(message)
        {
            
        }
    }
}
