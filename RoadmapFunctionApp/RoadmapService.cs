using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RoadmapFunctionApp
{
    public class RoadmapService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CosmosClient _cosmosClient;

        public RoadmapService(IHttpContextAccessor httpContextAccessor, CosmosClient cosmosClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _cosmosClient = cosmosClient;
        }

        public async Task<IActionResult> FetchRoadmaps(HttpRequest req, ILogger log)
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            Container container = _cosmosClient.GetContainer("competence", "roadmap");

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