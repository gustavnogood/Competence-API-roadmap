using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;


namespace RoadmapFunctionApp
{
    public class RoadmapHttpFunctions
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CosmosClient _cosmosClient;
        //private readonly ITokenValidator _tokenValidator;

        public RoadmapHttpFunctions(IHttpContextAccessor httpContextAccessor, CosmosClient cosmosClient)//, ITokenValidator tokenValidator)
        {
            _httpContextAccessor = httpContextAccessor;
            _cosmosClient = cosmosClient;
            //_tokenValidator = tokenValidator;
        }

        [FunctionName("CreateRoadmapFunction")]
        public async Task<IActionResult> CreateRoadmap(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "roadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var roadmapService = new RoadmapService(_httpContextAccessor, _cosmosClient);//, _tokenValidator);
                return await roadmapService.CreateRoadmap(req, log);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error creating roadmap");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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