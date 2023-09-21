using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Function;

public static class JWETokenGenerator
{
    [FunctionName("JWETokenGenerator")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
    {
        log.LogInformation($"C# HTTP trigger function processed a request");
        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = Config.Audience,
            Issuer = Config.Issuer,
            Claims = req.Query
                .Where(q => !string.Equals(q.Key, "code", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(q => q.Key, q => (object)q.Value),

            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(Config.SigningKey, SecurityAlgorithms.HmacSha256Signature),
            EncryptingCredentials = new EncryptingCredentials(Config.EncryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256)
        };

        string token = tokenHandler.CreateToken(tokenDescriptor);

        return new OkObjectResult(token);
    }

}


