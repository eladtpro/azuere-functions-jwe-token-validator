using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Kusto;
using Newtonsoft.Json.Linq;

namespace Azure.Functions;

public static class OpenAIUsage 
{
    [FunctionName("OpenAIUsage ")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "/{userId}")]
        HttpRequest req,
        [Kusto(Database:"ApiManagementGatewayLogs" ,
        KqlCommand = 
            @"
            declare query_parameters (userId:long);
            ApiManagementGatewayLogs
            | extend requestBody = parse_json(BackendRequestBody)
            | extend responseBody = parse_json(ResponseBody)
            | where OperationId == 'post-query' //completions_create'
            | extend user = responseBody.subcription.Subscription.id
            | extend model = responseBody.model
            | extend prompt_tokens = responseBody.usage.prompt_tokens
            ",
        KqlParameters = "@userId={userId}",
        Connection = "KustoConnectionString")]
        IAsyncEnumerable<JObject> usage )
    {
        //string user = req.Query["name"]

        IAsyncEnumerator<JObject> enumerator = usage .GetAsyncEnumerator();
        var productList = new List<JObject>();
        while (await enumerator.MoveNextAsync())
        {
            productList.Add(enumerator.Current);
        }
        await enumerator.DisposeAsync();
        return new OkObjectResult(productList);
    }
}
