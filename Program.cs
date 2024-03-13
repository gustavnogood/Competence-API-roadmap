using Microsoft.Azure.Cosmos;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Azure.Identity;


namespace Company.Function
{
    public static class RoadmapFunction
    {
        [FunctionName("CreateRoadmapFunction")]
        public static async Task<IActionResult> CreateRoadmap(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "roadmap")] HttpRequest req,
    ILogger log)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", new ManagedIdentityCredential());

            Container container = client.GetContainer("competence", "movies") ?? throw new NullReferenceException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            RoadmapRequest data = JsonConvert.DeserializeObject<RoadmapRequest>(requestBody);
            data.id = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(data.name))
            {
                ItemResponse<RoadmapRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.name));
                return new OkObjectResult(request.Resource);
            }
            return new OkObjectResult("Failed to upload, no roadmap found in request.");
        }

        [FunctionName("GetRoadmapFunction")]
        public static async Task<IActionResult> FetchRoadmaps(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req,
        ILogger log)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", new ManagedIdentityCredential());

            Container container = client.GetContainer("competence", "movies") ?? throw new NullReferenceException();

            FeedIterator<RoadmapResponse> queryResultSetIterator = container.GetItemQueryIterator<RoadmapResponse>();
            var result = new HashSet<RoadmapResponse>();
            while (queryResultSetIterator.HasMoreResults)
            {
                foreach (var roadmap in await queryResultSetIterator.ReadNextAsync())
                {
                    result.Add(roadmap);
                }
            }
            return new OkObjectResult(result);
        }
        public class RoadmapRequest
        {
            
            public String id;
            public String name;

            public String description;
        }

        public class RoadmapResponse
        {
            public String id;
            public String name;
            public String description;
        }
    }


}