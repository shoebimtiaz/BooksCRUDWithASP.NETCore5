using System.Globalization;
using System.Threading.Tasks;
using BooksCRUD.Data.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace BooksCRUD.Data.Services
{
    public class CosmosBookLogService : IBookLogService
    {
        private readonly IConfiguration _configuration;
        private readonly CosmosClient _cosmosClient;

        public CosmosBookLogService(IConfiguration configuration, CosmosClient cosmosClient)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
        }

        public async Task LogAsync(BookLog log)
        {
            var databaseName = _configuration["CosmosConfiguration:DatabaseName"];
            var containerName = _configuration["CosmosConfiguration:ContainerName"];

            var container = _cosmosClient.GetContainer(databaseName, containerName);
            var partitionKey = $"{log.Action}_{log.Timestamp:yyyyMM}";

            await container.CreateItemAsync(log, new PartitionKey(partitionKey));
        }
    }
}
