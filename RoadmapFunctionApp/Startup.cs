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
using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace Company.Function
{
    public static class MovieFunction
    {
        [FunctionName("AddUserFunction")]
        public static async Task<IActionResult> CreateUser(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
    ILogger log)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", "r0ppqOeMX7GTifP0vAF4G8w6zFUv5IS74hYqTYJMzGCC2dOb81MYHuwWSnWKsOiadJ7qpXSBZOnIACDbbRybHg==");

            Container container = client.GetContainer("competence", "users") ?? throw new NullReferenceException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            UserRequest data = JsonConvert.DeserializeObject<UserRequest>(requestBody);
            data.id = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(data.displayName))
            {
                ItemResponse<UserRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.displayName));
                return new OkObjectResult(request.Resource);
            }
            return new OkObjectResult("Failed to upload, no users found in request.");
        }

        [FunctionName("GetRoadmapFunction")]
        public static async Task<IActionResult> FetchRoadmap(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req,
        ILogger log)
        {
            CosmosClient client = new CosmosClient("https://cosmos-competence-test.documents.azure.com:443/", "KkFvBCDgkDSHI8oE01FLBM57NfVWkECInvQcafkb1aKlcfllf9UJvlKMrbBf2QsZUaVFmjtQrtuLACDbbwMVIQ==");

            Container container = client.GetContainer("competence", "roadmap") ?? throw new NullReferenceException();

            FeedIterator<RoadmapResponse> queryResultSetIterator = container.GetItemQueryIterator<RoadmapResponse>();
            var result = new HashSet<RoadmapResponse>();
            while (queryResultSetIterator.HasMoreResults)
            {
                foreach (var movie in await queryResultSetIterator.ReadNextAsync())
                {
                    result.Add(movie);
                }
            }
            return new OkObjectResult(result);
        }
        public class UserRequest
        {

            public String id;
            public String displayName;
        }

        public class RoadmapResponse
        {
            public String id;
            public String name;
            public String description;

        }
    }


}
