namespace DingFrame.Encryption
{
	using System;
	using System.Text;
	using System.Security.Cryptography;
	
	public static class RSAEncryption
	{
		 public static (string publicKey, string privateKey) GenerateKeys()
		{
			using RSACryptoServiceProvider rsa = new();
			return (Convert.ToBase64String(rsa.ExportRSAPublicKey()), Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
		}

		public static string Encrypt(string plainText, string publicKey)
		{
			using RSACryptoServiceProvider rsa = new();
			rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
			var encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), false);
			return Convert.ToBase64String(encrypted);
		}

		public static string Decrypt(string cipherText, string privateKey)
		{
			using RSACryptoServiceProvider rsa = new();
			rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
			var decrypted = rsa.Decrypt(Convert.FromBase64String(cipherText), false);
			return Encoding.UTF8.GetString(decrypted);
		}
	}
}