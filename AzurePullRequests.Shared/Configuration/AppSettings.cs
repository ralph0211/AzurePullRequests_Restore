using Microsoft.Extensions.Configuration;

namespace AzurePullRequests.Shared.Configuration
{
    public class AppSettings
    {
        private readonly IConfiguration _configuration;

        public AppSettings()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ProjectName => _configuration["ProjectName"];

        public string Repository => _configuration["Repository"];

        public string OrganizationName => _configuration["OrganizationName"];

        public string RestoreStateFile => _configuration["RestoreStateFile"];

        public AzureDevOpsAuth AzureDevOpsAuth => new AzureDevOpsAuth(_configuration);
    }

    public class AzureDevOpsAuth
    {
        private readonly IConfiguration _configuration;

        public AzureDevOpsAuth(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string TokenUrl => _configuration["AzureDevOpsAuth:TokenUrl"];

        public string BaseUrl => _configuration["AzureDevOpsAuth:BaseUrl"];

        public string PatToken => _configuration["AzureDevOpsAuth:PatToken"];
    }
}
