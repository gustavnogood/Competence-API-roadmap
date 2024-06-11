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
            log.LogInformation($"Request Method: {req.Method}");
            log.LogInformation($"Request Path: {req.Path}");
            log.LogInformation($"Request Headers: {JsonConvert.SerializeObject(req.Headers)}");

            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", new ManagedIdentityCredential());
            Container container = client.GetContainer("competence", "users") ?? throw new NullReferenceException("Container not found");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Request Body: {requestBody}");

            UserRequest data;
            try
            {
                data = JsonConvert.DeserializeObject<UserRequest>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to deserialize request body: {ex}");
                return new BadRequestObjectResult("Invalid request body");
            }

            if (!string.IsNullOrEmpty(data.DisplayName))
            {
                try
                {
                    log.LogInformation($"Attempting to upsert user with display name: {data.DisplayName}");
                    ItemResponse<UserRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.DisplayName));
                    log.LogInformation("User upserted successfully.");
                    return new OkObjectResult(request.Resource);
                }
                catch (Exception ex)
                {
                    log.LogError($"Failed to upsert user: {ex}");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }

            log.LogWarning("Failed to upload, no users found in request.");
            return new BadRequestObjectResult("Failed to upload, no users found in request.");
        }
    }
    public class UserRequest
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string RoadmapId { get; set; }
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

