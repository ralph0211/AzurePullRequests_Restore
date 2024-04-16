using AzurePullRequests.Restore.Enums;
using AzurePullRequests.Restore.Interfaces;
using AzurePullRequests.Shared.Configuration;
using AzurePullRequests.Shared.Contracts.Dtos;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AzurePullRequests.Restore.Services
{
    public class AzureDevOpsRestoreService : IAzureDevOpsRestoreService
    {
        private readonly AppSettings _appSettings;
        private readonly string _patToken;
        private readonly string _organizationUrl;
        private readonly string _backupFolder;

        public AzureDevOpsRestoreService(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _patToken = $"{_appSettings.AzureDevOpsAuth.PatToken}";
            _organizationUrl = $"{_appSettings.AzureDevOpsAuth.BaseUrl}{_appSettings.OrganizationName}";
            _backupFolder = $"C:/{_appSettings.ProjectName}/backup";
        }

        public async Task RestorePullRequestsAsync()
        {
            var restoreStateFile = _appSettings.RestoreStateFile;

            if (string.IsNullOrEmpty(restoreStateFile))
            {
                Console.WriteLine("RestoreStateFile parameter not set in appsettings!");
                throw new ArgumentNullException(nameof(restoreStateFile), "RestoreStateFile parameter not set in appsettings!");
            }

            var filePath = Path.Combine(_backupFolder, restoreStateFile);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} not found. Please enter valid file name!");
                throw new FileNotFoundException("File not found in backup folder", filePath);
            }

            // read state file
            var jsonState = File.ReadAllText(filePath);
            var state = JsonConvert.DeserializeObject<RepositoryState>(jsonState);

            foreach (var pullRequestState in state.PullRequests)
            {
                var apiUrl = $"{_organizationUrl}/_apis/git/repositories/{_appSettings.Repository}/pullrequests/{pullRequestState.Id}?api-version=7.1-preview.1";
                var pullRequestFileLocation = $"{state.BackupLocation}/{pullRequestState.Id}.json";
                var jsonPR = File.ReadAllText(pullRequestFileLocation);
                var pullRequest = JsonConvert.DeserializeObject<GitPullRequest>(jsonPR);

                // check if PR is in DevOps. If there update PR else create new
                if (await IsPullRequestInDevOps(pullRequestState.Id))
                {
                    // update existing
                    await RestoreDevOpsEntityAsync(RestoreType.Update, apiUrl, pullRequest);
                }
                else
                {
                    // create new
                    await RestoreDevOpsEntityAsync(RestoreType.Create, apiUrl, pullRequest);
                }
            }
        }

        private async Task<bool> IsPullRequestInDevOps(int pullRequestId)
        {
            var apiUrl = $"{_organizationUrl}/_apis/git/repositories/{_appSettings.Repository}/pullrequests/{pullRequestId}?api-version=7.1-preview.1";
            var pullRequest = await GetDevOpsEntitiesAsync<GitPullRequest>(apiUrl);
            if (pullRequest is not null) return true;
            return false;
        }

        private async Task<T> GetDevOpsEntitiesAsync<T>(string apiUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_patToken}")));

                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var entities = JsonConvert.DeserializeObject<T>(jsonContent);

                        return entities;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get entities. Status code: {response.StatusCode}");
                        throw new AzureDevOpsException($"Error occurred while getting entities.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        private async Task PatchDevOpsEntityAsync(string apiUrl, HttpContent content)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_patToken}")));

                    var response = await client.PatchAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Pull request successfully updated!");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get entities. Status code: {response.StatusCode}");
                        throw new AzureDevOpsException($"Error occurred while getting entities.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        private async Task RestoreDevOpsEntityAsync(RestoreType restoreType, string apiUrl, GitPullRequest pullRequest)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_patToken}")));

                    var response = new HttpResponseMessage();

                    switch (restoreType)
                    {
                        case RestoreType.Create:
                            var jsonPR = JsonConvert.SerializeObject(pullRequest);
                            HttpContent content = new StringContent(jsonPR, Encoding.UTF8, "application/json");
                            response = await client.PostAsync(apiUrl, content);
                            break;
                        case RestoreType.Update:
                            // update will not take certain fields o just leaving Title and Description for now.
                            // Comments updated in own endpoint
                            var updateObject = new
                            {
                                pullRequest.Title,
                                pullRequest.Description,
                            };
                            var prToUpdate = JsonConvert.SerializeObject(updateObject);
                            HttpContent content1 = new StringContent(prToUpdate, Encoding.UTF8, "application/json");
                            response = await client.PatchAsync(apiUrl, content1);
                            break;
                        default:
                            throw new ArgumentException("Invalid restoreType value");
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Pull request successfully restored!");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get entities. Status code: {response.StatusCode}");
                        throw new AzureDevOpsException($"Error occurred while getting entities.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
