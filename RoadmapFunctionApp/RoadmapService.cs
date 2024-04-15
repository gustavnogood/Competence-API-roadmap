using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RoadmapFunctionApp
{
public class RoadmapService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenValidator tokenValidator;
    private readonly CosmosClient _cosmosClient;

    public RoadmapService(IHttpContextAccessor httpContextAccessor, CosmosClient cosmosClient, ITokenValidator tokenValidator)
    {
        _httpContextAccessor = httpContextAccessor;
        this.tokenValidator = tokenValidator;
        _cosmosClient = cosmosClient;
    }

        public async Task<IActionResult> CreateRoadmap(HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("CreateRoadmap function started.");

                var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
                tokenValidator.ValidateToken(accessToken);
                var userInfo = tokenValidator.GetUserInfoFromToken(accessToken);
                Container container = _cosmosClient.GetContainer("competence", "roadmap");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                RoadmapRequest data = JsonConvert.DeserializeObject<RoadmapRequest>(requestBody);
                data.id = userInfo.ToString();


                if (!string.IsNullOrEmpty(data.name))
                {
                    ItemResponse<RoadmapRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.name));
                    log.LogInformation("CreateRoadmap function completed successfully.");
                    return new OkObjectResult(request.Resource);
                }
                else
                {
                    return new BadRequestObjectResult("Failed to upload: no roadmap name found in the request.");
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while executing the CreateRoadmap function.");
                return new BadRequestObjectResult("An error occurred.");
            }
        }



        public async Task<IActionResult> FetchRoadmaps(HttpRequest req, ILogger log)
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            tokenValidator.ValidateToken(accessToken);
            // Rest of the method
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