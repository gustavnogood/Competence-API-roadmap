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
using Microsoft.Extensions.Azure;

namespace Company.Function
{
    public static class RoadmapFunction
    {
        private static readonly string cosmosEndpoint = "https://cosmos-competence-test.documents.azure.com:443/";

        private static CosmosClient cosmosClient;

        static RoadmapFunction()
        {
            InitializeCosmosClient();

            void InitializeCosmosClient()
            {
                // Obtain access token using Managed Identity
                var tokenCredential = new DefaultAzureCredential();

                // Initialize Cosmos Client with access token and endpoint
                cosmosClient = new CosmosClient(cosmosEndpoint, tokenCredential);
            }
        }

        [FunctionName("CreateRoadmapFunction")]
        public static async Task<IActionResult> CreateRoadmap(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "roadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                Container container = cosmosClient.GetContainer("competence", "roadmap");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                RoadmapRequest data = JsonConvert.DeserializeObject<RoadmapRequest>(requestBody);
                data.id = Guid.NewGuid().ToString();

                if (!string.IsNullOrEmpty(data.name))
                {
                    ItemResponse<RoadmapRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.name));
                    return new OkObjectResult(request.Resource);
                }
                return new BadRequestObjectResult("Failed to upload: no roadmap name found in the request.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error creating roadmap");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [FunctionName("GetRoadmapFunction")]
        public static async Task<IActionResult> FetchRoadmaps(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                Container container = cosmosClient.GetContainer("competence", "roadmap");

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
            catch (Exception ex)
            {
                log.LogError(ex, "Error fetching roadmaps");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        public class RoadmapRequest
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
            public List<Skill> skills { get; set; }
        }

        public class Skill
        {
            public string skillId { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }

        public class RoadmapResponse
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public List<Role> roles { get; set; }
        }
    }
}