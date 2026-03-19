namespace DingFrame.Encryption
{
	using System;
	using System.Text;
	using System.Security.Cryptography;
	
	public static class HashEncryption
	{
		public static string ComputeHash(string input) => ComputeHash(Encoding.UTF8.GetBytes(input));

		public static string ComputeHash(byte[] buffer)
		{
			using SHA256 sha256 = SHA256.Create();
			byte[] bytes = sha256.ComputeHash(buffer);
			return Convert.ToBase64String(bytes);
		}
	}
}