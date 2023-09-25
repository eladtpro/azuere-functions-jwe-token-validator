using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Net;
using Newtonsoft.Json.Linq;

namespace MyNamespace;
public static class JWETokenValidator
{
    [FunctionName(nameof(JWETokenValidator))]
    [OpenApiOperation(operationId: "Run")]
    //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody("application/json", typeof(JObject), Description = "JSON request body containing { hours, capacity}")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        // extract token from input
        string token = req.Query["token"];
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        token = token ?? data?.token;
        // if the token not found in the input - try the header
        token = token ?? (req.Headers.TryGetValue("Authorization", out var authHeaderValues) ?
            authHeaderValues[0].Replace("Bearer ", ""): null);

        if(string.IsNullOrEmpty(token))
            return new BadRequestResult();


        JsonWebTokenHandler handler = new JsonWebTokenHandler();
        TokenValidationResult result = await handler.ValidateTokenAsync(token,
            new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidAudience = Config.Audience,
            ValidIssuer = Config.Issuer,
            IssuerSigningKey = Config.SigningKey,
            TokenDecryptionKey = Config.EncryptionKey
        });


        var response = new
        {
            Prefix = token.Substring(0, Math.Min(token.Length, 10)),
            result.IsValid,
            result.Claims
        };
        return new OkObjectResult(response);
    }
}
