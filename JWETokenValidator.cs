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

namespace Microsoft.Function;
public static class JWETokenValidator
{
    [FunctionName("JWETokenValidator")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string token = req.Query["token"];
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        token = token ?? data?.token;
        token = token ?? (req.Headers.TryGetValue("Authorization", out var authHeaderValues) ?
            authHeaderValues[0].Replace("Bearer ", ""): null);

        if(string.IsNullOrEmpty(token))
            return new BadRequestResult();

        JsonWebTokenHandler handler = new JsonWebTokenHandler();
        TokenValidationResult result = handler.ValidateToken(token, new TokenValidationParameters
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
