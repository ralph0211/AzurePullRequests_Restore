using AzurePullRequests.Shared.Configuration;
using AzurePullRequests.Shared.Contracts.Dtos;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using AzurePullRequests.Backup.Interfaces;

namespace AzurePullRequests.Backup.Services
{
    public class AzureDevOpsBackupService : IAzureDevOpsBackupService
    {
        private readonly AppSettings _appSettings;
        private readonly string _patToken;
        private readonly string _organizationUrl;
        private readonly string _backupFolder;

        public AzureDevOpsBackupService(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _patToken = $"{_appSettings.AzureDevOpsAuth.PatToken}";
            _organizationUrl = $"{_appSettings.AzureDevOpsAuth.BaseUrl}{_appSettings.OrganizationName}";
            _backupFolder = $"C:/{_appSettings.ProjectName}/backup";
        }

        public async Task<PullRequestsResponse> BackupActivePullRequestsAsync()
        {
            var apiUrl = $"{_organizationUrl}/_apis/git/repositories/{_appSettings.Repository}/pullrequests?searchCriteria.status=active&api-version=7.1-preview.1";

            try
            {
                var gitPullRequests = await GetDevOpsEntitiesAsync<PullRequestsResponse>(apiUrl);

                if (gitPullRequests is not null)
                {
                    var previousRepositoryState = GetLatestState(_backupFolder);
                    if (previousRepositoryState is null)
                    {
                        previousRepositoryState = new List<RepositoryState>();
                    }

                    var currentRepositoryState = new List<RepositoryState>();

                    foreach (var pullRequest in gitPullRequests.Value)
                    {
                        var epoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var comments = await GetPullRequestCommentThreadsAsync(pullRequest.PullRequestId, epoch);

                        var prState = new RepositoryState
                        {
                            Id = pullRequest.PullRequestId,
                            LastMergeCommit = pullRequest.LastMergeCommit?.Author?.Date ?? default,
                            ThreadLastUpdated = comments?.Value?.Max(x => x.LastUpdatedDate) ?? default,
                        };

                        //var previousPrState = previousRepositoryState.Find(pr => pr.Id == pullRequest.PullRequestId);
                        //var prStateHasChanged = false;

                        //if (previousPrState is null)
                        //{
                        //    prStateHasChanged = true;
                        //}
                        //else
                        //{
                        //    prStateHasChanged = prState.LastMergeCommit != previousPrState.LastMergeCommit || 
                        //        prState.ThreadLastUpdated != previousPrState.ThreadLastUpdated;
                        //}

                        var prJsonString = JsonConvert.SerializeObject(pullRequest);
                        var filePath = $"C:/{_appSettings.ProjectName}/backup/{epoch}/{pullRequest.PullRequestId}.json";
                        WriteJsonToFile(prJsonString, filePath);

                        currentRepositoryState.Add(prState);
                    }

                    if (RepositoryStateHasChanged(previousRepositoryState, currentRepositoryState))
                    {
                        var stateFilePath = $"C:/{_appSettings.ProjectName}/backup/{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.json";
                        var stateString = JsonConvert.SerializeObject(currentRepositoryState);
                        WriteJsonToFile(stateString, stateFilePath);
                    }
                    return gitPullRequests;
                }
                else
                {
                    Console.WriteLine($"Error occurred. Pull requests are null!");
                    return null; // or throw an exception
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // or throw an exception
            }
        }

        private async Task<PullRequestCommentThreadResponse> GetPullRequestCommentThreadsAsync(int pullRequestId, long epoch)
        {
            var organizationUrl = $"{_appSettings.AzureDevOpsAuth.BaseUrl}{_appSettings.OrganizationName}";
            var apiUrl = $"{organizationUrl}/_apis/git/repositories/{_appSettings.Repository}/pullrequests/{pullRequestId}/threads?api-version=7.1-preview.1";

            try
            {
                using (var client = new HttpClient())
                {
                    var commentThreads = await GetDevOpsEntitiesAsync<PullRequestCommentThreadResponse>(apiUrl);

                    if (commentThreads is not null)
                    {
                        foreach (var commentThread in commentThreads.Value)
                        {
                            // backup to json file
                            var threadJsonString = JsonConvert.SerializeObject(commentThread);
                            var filePath = $"C:/{_appSettings.ProjectName}/backup/{epoch}/{pullRequestId}/threads.json";
                            WriteJsonToFile(threadJsonString, filePath);

                        }
                        return commentThreads;
                    }
                    else
                    {
                        Console.WriteLine($"Error occurred. Thread is null!");
                        return null; // or throw an exception
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null; // or throw an exception
            }
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

        private void WriteJsonToFile(string jsonString, string filePath)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(filePath, jsonString);

                Console.WriteLine($"JSON data successfully written to file: {filePath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to write JSON data to file: {ex.Message}");
            }
        }

        private List<RepositoryState> GetLatestState(string folderPath)
        {
            try
            {
                var jsonFiles = Directory.GetFiles(folderPath, "*.json")
                    .Select(Path.GetFileName)
                    .ToList();
                jsonFiles.Sort();

                var latestFile = jsonFiles.LastOrDefault();

                if (latestFile != null)
                {
                    var jsonContent = File.ReadAllText(Path.Combine(folderPath, latestFile));
                    var repositoryState = JsonConvert.DeserializeObject<List<RepositoryState>>(jsonContent);

                    return repositoryState;
                }
                else
                {
                    Console.WriteLine("No JSON files found in the folder.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        private bool RepositoryStateHasChanged(List<RepositoryState> previousState, List<RepositoryState> newState)
        {
            if (previousState.Count != newState.Count)
                return true;

            foreach (var state in previousState)
            {
                var currentState = newState.Find(pr => pr.Id == state.Id);
                if (currentState is null)
                    return true;

                if (state.ThreadLastUpdated != currentState.ThreadLastUpdated || state.LastMergeCommit != currentState.LastMergeCommit) 
                    return true;
            }
            return false;
        }
    }
}
