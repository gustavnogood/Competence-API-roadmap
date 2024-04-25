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

namespace Company.Function
{
    public static class GetFunction
    {
        
        [FunctionName("GetRoadmapFunction")]
        public static async Task<IActionResult> FetchRoadmap(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req,
        ILogger log)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", "r0ppqOeMX7GTifP0vAF4G8w6zFUv5IS74hYqTYJMzGCC2dOb81MYHuwWSnWKsOiadJ7qpXSBZOnIACDbbRybHg==");

            Container container = client.GetContainer("competence", "roadmap") ?? throw new NullReferenceException();

            FeedIterator<RoadmapResponse> queryResultSetIterator = container.GetItemQueryIterator<RoadmapResponse>();
            var result = new List<RoadmapResponse>();

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
    public static class AddUserFunction {
        [FunctionName("AddUserFunction")]
        public static async Task<IActionResult> CreateUser(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("CreateUser function started.");

            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", "r0ppqOeMX7GTifP0vAF4G8w6zFUv5IS74hYqTYJMzGCC2dOb81MYHuwWSnWKsOiadJ7qpXSBZOnIACDbbRybHg==");

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
    }

    public class RoadmapResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Role> roles { get; set; }
    }

    public class Role
    {
        public string roleId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<Skill> skills { get; set; } // New property
    }

    public class Skill
    {
        public string skillId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}

