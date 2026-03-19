namespace DingFrame.Encryption
{
	using System;
	using System.IO;
	using System.Text;
	using System.Security.Cryptography;
	using System.Threading.Tasks;
	using DingFrame.Utils;

	public interface IAESKeyFragment
	{
		string Key{get;}
	}

	public interface IAESIVFragment
	{
		int IV{get;}
	}

	public static class AESEncryption
	{
		public static byte[] Encrypt(string plainText, char[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			using ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
			return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
		}
		public static void Encrypt(string plainText, FileStream outputFileStream, char[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			using ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			using var cryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write);
			using var writer = new StreamWriter(cryptoStream);
			writer.Write(plainText);
			writer.Flush();
			cryptoStream.FlushFinalBlock();
		}
		public static async Task EncryptAsync(string plainText, FileStream outputFileStream, char[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			using ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			using var cryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write);
			using var writer = new StreamWriter(cryptoStream);
			await writer.WriteAsync(plainText);
		}

		public static string Decrypt(byte[] cipherBytes, char[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			using ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
			return Encoding.UTF8.GetString(plainBytes);
		}
		public static string Decrypt(FileStream inputFileStream, char[] key, byte[] iv)
		{
			using Aes aes = Aes.Create();
			using ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			using var bufferedStream = new BufferedStream(inputFileStream);
			using var cryptoStream = new CryptoStream(bufferedStream, decryptor, CryptoStreamMode.Read);
			using var reader = new StreamReader(cryptoStream);
			return reader.ReadToEnd();
		}
		public static async Task<string> DecryptAsync(FileStream inputFileStream, char[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			using var decryptor = aes.CreateDecryptor(Encoding.UTF8.GetBytes(ArrayUtils.ResizeArray(key, 32)), ArrayUtils.ResizeArray(iv, 16));
			using var bufferedStream = new BufferedStream(inputFileStream);
			using var cryptoStream = new CryptoStream(bufferedStream, decryptor, CryptoStreamMode.Read);
			using var reader = new StreamReader(cryptoStream);
			return await reader.ReadToEndAsync();
		}
	}
}