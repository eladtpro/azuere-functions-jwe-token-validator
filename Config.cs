using System;
using Microsoft.IdentityModel.Tokens;

namespace MyNamespace;
public static class Config
{
	public static SecurityKey SigningKey =>
		new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWE_SIGNING_KEY")));

	public static SecurityKey EncryptionKey =>
		new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWE_ENCRYPTION_KEY")));

	public static string Audience => Environment.GetEnvironmentVariable("JWE_AUDIENCE");

	public static string Issuer => Environment.GetEnvironmentVariable("JWE_ISSUER");
}


