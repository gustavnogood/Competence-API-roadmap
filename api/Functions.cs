using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;

namespace Company.Function
{
    public static class GetFunction
    {
        [FunctionName("GetRoadmapFunction")]
        public static async Task<IActionResult> FetchRoadmap(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", new ManagedIdentityCredential());
            Container container = client.GetContainer("competence", "roadmap") ?? throw new NullReferenceException();

            FeedIterator<Node> queryResultSetIterator = container.GetItemQueryIterator<Node>();
            var result = new List<Node>();

            while (queryResultSetIterator.HasMoreResults)
            {
                foreach (var roadmap in await queryResultSetIterator.ReadNextAsync())
                {
                    result.Add(roadmap);
                }
            }
            return new OkObjectResult(result);
        }
    }
    public static class AddUserFunction
    {
        [FunctionName("AddUserFunction")]
        public static async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("CreateUser function started.");

            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", new ManagedIdentityCredential());

            Container container = client.GetContainer("competence", "users") ?? throw new NullReferenceException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            UserRequest data = JsonConvert.DeserializeObject<UserRequest>(requestBody);

            if (!string.IsNullOrEmpty(data.displayName))
            {
                log.LogInformation($"Attempting to upsert user with display name: {data.displayName}");
                ItemResponse<UserRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.displayName));
                log.LogInformation("User upserted successfully.");
                return new OkObjectResult(request.Resource);
            }
            log.LogWarning("Failed to upload, no users found in request.");
            return new OkObjectResult("Failed to upload, no users found in request.");
        }
    }
    public class UserRequest
    {
        public string id;
        public string displayName;
        public string roadmapId;
    }

    public class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Node> Children { get; set; }
    }

    public class Root
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Node> Children { get; set; }
        public string Rid { get; set; }
        public string Self { get; set; }
        public string Etag { get; set; }
        public string Attachments { get; set; }
        public int Ts { get; set; }
    }
}

