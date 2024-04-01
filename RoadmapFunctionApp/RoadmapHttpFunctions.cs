using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RoadmapFunctionApp
{
    public static class RoadmapHttpFunctions
    {
        [FunctionName("CreateRoadmapFunction")]
        public static async Task<IActionResult> CreateRoadmap(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "roadmap")] HttpRequest req,
            ILogger log)
        {
            try
            {
                return await RoadmapService.CreateRoadmap(req, log);
            }
            catch (System.Exception ex)
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
                return await RoadmapService.FetchRoadmaps(req, log);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Error fetching roadmaps");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}