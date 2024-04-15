using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

[assembly: FunctionsStartup(typeof(RoadmapFunctionApp.Startup))]

namespace RoadmapFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var cosmosEndpoint = "https://cosmos-competence-test.documents.azure.com:443/";
            var tokenCredential = new DefaultAzureCredential();
            var cosmosClient = new CosmosClient(cosmosEndpoint, tokenCredential);

            builder.Services.AddSingleton(cosmosClient);
            builder.Services.AddHttpContextAccessor();
/*             builder.Services.AddSingleton<ITokenValidator, TokenValidator>(); */
/*             builder.Services.AddSingleton<IUpsertUser, UpsertUser>(); */

        }
    }
}