namespace DingFrame.Utils
{
	using System;
	using System.Text;
	using System.IO;

	public static class StringUtils
	{
		public static string GenerateRandomChars(int charCount)
		{
			if (charCount <= 0) return null;

			const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
			Random random = new Random();
			StringBuilder sb = new StringBuilder(4);

			for (int i = 0; i < charCount; i++) sb.Append(charset[random.Next(charset.Length)]);

			return sb.ToString();
		}

		public static string TruncateString(string input, int length, string placeStr = "..")
		{
			if (string.IsNullOrEmpty(input) || length < 0 || input.Length <= length)
			{
				return input;
			}

			return input[..length] + placeStr;
		}

	}
}