using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace Company.Function
{
    public class GetResumeCounter
    {
        private readonly ILogger<GetResumeCounter> _logger;

        public GetResumeCounter(ILogger<GetResumeCounter> logger)
        {
            _logger = logger;
        }

        [Function("GetResumeCounter")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("AzureResumeConnectionString"));
            var container = cosmosClient.GetContainer("azureresume", "counter");

            // Read the counter document
            var response = await container.ReadItemAsync<Counter>("1", new PartitionKey("1"));
            var counter = response.Resource;

            // Update counter
            counter.Count += 1;

            // Save the updated counter
            await container.ReplaceItemAsync(counter, counter.Id, new PartitionKey(counter.PartitionKey));

            // Create response
            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await httpResponse.WriteStringAsync(JsonConvert.SerializeObject(counter));

            return httpResponse;
        }
    }
}