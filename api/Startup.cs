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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

namespace Competence.Function
{
    public static class ServiceProviderBuilder
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void Initialize(IConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            var cosmosDbConnectionString = config.GetConnectionString("CosmosDBConnection");
            serviceCollection.AddSingleton(x => new CosmosClient(cosmosDbConnectionString));
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
    public class Startup
{
    public void Configure(IFunctionsHostBuilder builder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        ServiceProviderBuilder.Initialize(config);
    }
}
    public class GetFunction
    {
        private readonly ILogger<GetFunction> _logger;
        private readonly CosmosClient _client;

        public GetFunction(ILogger<GetFunction> logger, IConfiguration config)
        {
            _logger = logger;
            ServiceProviderBuilder.Initialize(config);
            _client = ServiceProviderBuilder.ServiceProvider.GetRequiredService<CosmosClient>();
        }
        [FunctionName("GetRoadmapFunction")]
        public async Task<IActionResult> FetchRoadmap(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roadmap")] HttpRequest req)
        {

            _logger.LogInformation("FetchRoadmap function started.");
            Container container = _client.GetContainer("competence", "roadmap") ?? throw new NullReferenceException();
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
    public class AddUserFunction
    {
        private readonly ILogger<AddUserFunction> _logger;
        private readonly CosmosClient _client;

        public AddUserFunction(ILogger<AddUserFunction> logger, CosmosClient client)
        {
            _logger = logger;
            _client = client;
        }
        [FunctionName("AddUserFunction")]
        public async Task<IActionResult> CreateUser(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("CreateUser function started.");

            Container container = _client.GetContainer("competence", "users") ?? throw new NullReferenceException();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            UserRequest data = JsonConvert.DeserializeObject<UserRequest>(requestBody);

            if (!string.IsNullOrEmpty(data.displayName))
            {
                _logger.LogInformation($"Attempting to upsert user with display name: {data.displayName}");
                ItemResponse<UserRequest> request = await container.UpsertItemAsync(data, new PartitionKey(data.displayName));
                _logger.LogInformation("User upserted successfully.");
                return new OkObjectResult(request.Resource);
            }
            _logger.LogWarning("Failed to upload, no users found in request.");
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

