using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RoadmapFunctionApp
{
    public static class RoadmapService
    {
        private static readonly string cosmosEndpoint = "https://cosmos-competence-test.documents.azure.com:443/";
        private static CosmosClient cosmosClient;

        static RoadmapService()
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

        public static async Task<IActionResult> CreateRoadmap(HttpRequest req, ILogger log)
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

        public static async Task<IActionResult> FetchRoadmaps(HttpRequest req, ILogger log)
        {
            Container container = cosmosClient.GetContainer("competence", "roadmap");

            FeedIterator<RoadmapResponse> queryResultSetIterator = container.GetItemQueryIterator<RoadmapResponse>();
            var result = new System.Collections.Generic.HashSet<RoadmapResponse>();
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
}
