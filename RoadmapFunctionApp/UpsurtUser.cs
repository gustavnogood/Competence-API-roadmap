/* using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace RoadmapFunctionApp
{
    public class UpsertUser : IUpsertUser
    {
        private readonly ITokenValidator _tokenService;
        private readonly Container _usersContainer;

        public UpsertUser(ITokenValidator tokenService, CosmosClient cosmosClient)
        {
            _tokenService = tokenService;
            _usersContainer = cosmosClient.GetContainer("RoadmapDb", "Users");
        }

        public async Task Execute(string accessToken)
        {
            // Validate the token and extract user info from it
            var userInfo = _tokenService.GetUserInfoFromToken(accessToken);

            // Upsert user info in "users" container
            await _usersContainer.UpsertItemAsync(userInfo, new PartitionKey(userInfo.UserId));
        }
    }
} */