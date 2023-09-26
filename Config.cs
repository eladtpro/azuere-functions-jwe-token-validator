using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace MyNamespace;
public class Config
{

	private static Lazy<Config> current = new Lazy<Config>(() => {

		var encryptionKid = Environment.GetEnvironmentVariable("JWE_ENCRYPTION_KEY");
		var signingKid = Environment.GetEnvironmentVariable("JWE_SIGNING_KEY");
		
		var encryptionKey = RSA.Create(3072); // public key for encryption, private key for decryption
		var signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256); // private key for signing, public key for validation

		return new Config(){
			Audience = Environment.GetEnvironmentVariable("JWE_AUDIENCE"),
			Issuer = Environment.GetEnvironmentVariable("JWE_ISSUER"),
			PrivateEncryptionKey = new RsaSecurityKey(encryptionKey) {KeyId = encryptionKid},
			PublicEncryptionKey = new RsaSecurityKey(encryptionKey.ExportParameters(false)) {KeyId = encryptionKid},
			PrivateSigningKey = new ECDsaSecurityKey(signingKey) {KeyId = signingKid},
			PublicSigningKey = new ECDsaSecurityKey(ECDsa.Create(signingKey.ExportParameters(false))) {KeyId = signingKid}
		};
	});
	
	public static Config Current => current.Value;

    public string Audience { get; private set;}
	public string Issuer { get; private set;}
    public RsaSecurityKey PrivateEncryptionKey { get; private set;}
    public RsaSecurityKey PublicEncryptionKey { get; private set;}
    public ECDsaSecurityKey PrivateSigningKey { get; private set;}
    public ECDsaSecurityKey PublicSigningKey { get; private set;}

	private Config()
	{
	}
}


