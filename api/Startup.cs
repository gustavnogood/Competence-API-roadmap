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
    public class GetFunction
    {
        private readonly ILogger<GetFunction> _logger;

        public GetFunction(ILogger<GetFunction> logger)
        {
            _logger = logger;
        }
        [FunctionName("GetRoadmapFunction")]
        public async Task<IActionResult> FetchRoadmap(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req)
        {

            _logger.LogInformation("FetchRoadmap function started.");
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", "r0ppqOeMX7GTifP0vAF4G8w6zFUv5IS74hYqTYJMzGCC2dOb81MYHuwWSnWKsOiadJ7qpXSBZOnIACDbbRybHg==");

            Container container = client.GetContainer("competence", "roadmap") ?? throw new NullReferenceException();
            _logger.LogInformation("Container retrieved.");

            FeedIterator<Node> queryResultSetIterator = container.GetItemQueryIterator<Node>();
            var result = new List<Node>();

            while (queryResultSetIterator.HasMoreResults)
            {
                foreach (var roadmap in await queryResultSetIterator.ReadNextAsync())
                {
                    _logger.LogInformation($"Adding roadmap with id: {roadmap.Id}");
                    result.Add(roadmap);
                }
            }
            _logger.LogInformation("FetchRoadmap function completed.");
            return new OkObjectResult(result);
        }
    }
    public class AddUserFunction {
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
    public string _rid { get; set; }
    public string _self { get; set; }
    public string _etag { get; set; }
    public string _attachments { get; set; }
    public int _ts { get; set; }
}
}

