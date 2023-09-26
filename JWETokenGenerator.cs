using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace MyNamespace;

public static class JWETokenGenerator
{
    [FunctionName(nameof(JWETokenGenerator))]
    [OpenApiOperation(operationId: "JWETokenGenerator")]
    //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/text", bodyType: typeof(string), Description = "The OK response message containing a JSON result.")]
    [OpenApiParameter("sub", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The subject of the token, e.g. user@domain.org")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation($"C# HTTP trigger function processed a request");
        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = Config.Current.Audience,
            Issuer = Config.Current.Issuer,
            Claims = req.Query
                .Where(q => !string.Equals(q.Key, "code", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(q => q.Key, q => (object)q.Value),

            Expires = DateTime.UtcNow.AddDays(1),

            // private key for signing
            SigningCredentials = new SigningCredentials(Config.Current.PrivateSigningKey, SecurityAlgorithms.EcdsaSha256),

            // public key for encryption
            EncryptingCredentials = new EncryptingCredentials(Config.Current.PublicEncryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512)
        };

        string token = tokenHandler.CreateToken(tokenDescriptor);

        return new OkObjectResult(token);
    }

}


