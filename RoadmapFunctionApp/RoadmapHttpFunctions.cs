using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;


namespace RoadmapFunctionApp
{
    public class RoadmapHttpFunctions
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CosmosClient _cosmosClient;

        public RoadmapHttpFunctions(IHttpContextAccessor httpContextAccessor, CosmosClient cosmosClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _cosmosClient = cosmosClient;
        }

        [FunctionName("SaveRoadmapFunction")]
        public async Task<IActionResult> SaveRoadmap(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "saveroadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                ClaimsPrincipal user = ClaimsPrincipalParser.Parse(req);
                var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "sub") ?? user.Claims.FirstOrDefault(c => c.Type == "oid");
                if (userIdClaim == null)
                {
                    log.LogError("User ID claim not found");
                    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation($"Request Body: {requestBody}"); // Log the request body

                var userId = userIdClaim.Value;
                log.LogInformation($"User ID: {userId}"); // Log the user ID

                var roadmapService = new RoadmapService(_httpContextAccessor, _cosmosClient);
                return await roadmapService.SaveRoadmap(req, userId, log);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error saving roadmap");
                throw;
            }
        }


        [FunctionName("GetRoadmapFunction")]
        public async Task<IActionResult> FetchRoadmaps(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var roadmapService = new RoadmapService(_httpContextAccessor, _cosmosClient);//, _tokenValidator);
                return await roadmapService.FetchRoadmaps(req, log);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error fetching roadmaps");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}