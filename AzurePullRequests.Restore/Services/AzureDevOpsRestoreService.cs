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
                // check if PR is in DevOps. If there update PR else create new
                HttpContent content = new StringContent(jsonPR, Encoding.UTF8, "application/json");
                if (await IsPullRequestInDevOps(pullRequestState.Id))
                {
                    // update existing
                    await RestoreDevOpsEntityAsync(RestoreType.Update, apiUrl, content);
                }
                else
                {
                    // create new
                    await RestoreDevOpsEntityAsync(RestoreType.Create, apiUrl, content);
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

        private async Task RestoreDevOpsEntityAsync(RestoreType restoreType, string apiUrl, HttpContent content)
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
                            response = await client.PostAsync(apiUrl, content);
                            break;
                        case RestoreType.Update:
                            response = await client.PatchAsync(apiUrl, content);
                            break;
                        default:
                            throw new ArgumentException("Invalid restoreType value");
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Pull request successfully created!");
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
