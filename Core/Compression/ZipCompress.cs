namespace DingFrame.Compress
{
	using System.IO;
	using System.IO.Compression;
	using System.Collections.Generic;

	public static class ZipCompress
	{
		public static void Compress(string[] filePaths, string zipFilePath, string[] fileRelativePaths)
		{
			if (filePaths == null || filePaths.Length == 0) return;

			if (fileRelativePaths != null && filePaths.Length != fileRelativePaths.Length)
			{
				DLogger.Error("filePaths != fileRelativePaths", "ZipCompress.Compress");
				return;
			}

			using FileStream zipToOpen = new(zipFilePath, FileMode.Create);
			using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);

			for (int i = 0; i < filePaths.Length; ++i)
			{
				string filePath = filePaths[i];
				string entryName = fileRelativePaths == null ? Path.GetFileName(filePath) : fileRelativePaths[i];
				archive.CreateEntryFromFile(filePath, entryName);
			}
		}

		public static void Decompress(string zipFilePath, string destinationFolder) => ZipFile.ExtractToDirectory(zipFilePath, destinationFolder, true);

		public static Dictionary<string, byte[]> DecompressInMemory(string zipFilePath)
		{
			Dictionary<string, byte[]> dic = new();

			using FileStream zipToOpen = new(zipFilePath, FileMode.Open);
			using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Read);

			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				using Stream entryStream = entry.Open();
				using MemoryStream memoryStream = new ();

				entryStream.CopyTo(memoryStream);
				memoryStream.Position = 0;

				dic[entry.FullName] = memoryStream.ToArray();
			}

			return dic;
		}
	}
}