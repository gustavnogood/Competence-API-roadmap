using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

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

        public async Task Execute(string accessToken) //TODO: change the name to a better name
        {
            // Validate the token and extract user info from it
            var userInfo = _tokenService.GetUserInfoFromToken(accessToken);

            // Upsert user info in "users" container
            await _usersContainer.UpsertItemAsync(userInfo, new PartitionKey(userInfo.UserId));
        }

        void IUpsertUser.UpsertUser(string accessToken)
        {
            throw new System.NotImplementedException();
        }
    }
}