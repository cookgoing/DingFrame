namespace DingFrame.Utils
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using UnityEngine;

	public static class FileUtils
	{
		public static void ClearDirectory(string dirPath, bool ignoreAuth, params string[] ignoreFilePath)
		{
			if (!Directory.Exists(dirPath)) return;

			DirectoryInfo directory = new(dirPath);
			foreach (FileInfo file in directory.GetFiles())
			{
				if (ignoreFilePath?.Length > 0 && Array.FindIndex(ignoreFilePath, path => PathUtils.ToUnixPath(path) == PathUtils.ToUnixPath(file.FullName)) >= 0) continue;
				
				if (ignoreAuth) File.SetAttributes(file.FullName, FileAttributes.Normal);
				file.Delete();
			}
			foreach (DirectoryInfo subDirectory in directory.GetDirectories())
			{
				ClearDirectory(subDirectory.FullName, ignoreAuth);
				subDirectory.Delete(true);
			}
		}

		public static bool IsPersistentFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return false;
			
			string fullPath = Path.GetFullPath(filePath).Replace("\\", "/");
			string persistentPath = Path.GetFullPath(Application.persistentDataPath).Replace("\\", "/");
			return fullPath.StartsWith(persistentPath);
		}

		public static string DeduplicateFilePath(string filePath)
		{
			string dirPath = Path.GetDirectoryName(filePath);
			string fielName = Path.GetFileName(filePath);
			string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fielName);
			string ext = Path.GetExtension(fielName);
			string fullPath = PathUtils.UnixPathCombine(dirPath, fielName);
			int count = 1;
			while (File.Exists(fullPath))
			{
				string newFileName = $"{fileNameWithoutExt}({count++}){ext}";
				fullPath = PathUtils.UnixPathCombine(dirPath, newFileName);
			}
			return fullPath;
		}

		public static void CopyFilesInDirectory(string sourceDirPath, string targetDirPath, bool allowDuplicates, bool onlyTopDir, Dictionary<string, string> changedFileName = null)
		{
			if (!Directory.Exists(sourceDirPath)) throw new Exception($"the dir path is not exist. sourceDirPath: {sourceDirPath}; targetDirPath: {targetDirPath}");

			if (!Directory.Exists(targetDirPath)) Directory.CreateDirectory(targetDirPath);

			foreach (string filePath in Directory.GetFiles(sourceDirPath))
			{
				string relativePath = Path.GetRelativePath(sourceDirPath, filePath);
				string targetPath = PathUtils.UnixPathCombine(targetDirPath, relativePath);
				if (allowDuplicates)
				{
					string oldPath = targetPath;
					targetPath = DeduplicateFilePath(targetPath);
					if (oldPath != targetPath && changedFileName != null) changedFileName[Path.GetFileName(oldPath)] = Path.GetFileName(targetPath);
				}
				File.Copy(filePath, targetPath, true);
			}

			if (onlyTopDir) return;

			foreach (string dirPath in Directory.GetDirectories(sourceDirPath))
			{
				string relativePath = Path.GetRelativePath(sourceDirPath, dirPath);
				string targetPath = PathUtils.UnixPathCombine(targetDirPath, relativePath);
				if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

				CopyFilesInDirectory(dirPath, targetPath, allowDuplicates, onlyTopDir, changedFileName);
			}
		}

		public static string CopyFileToDirectory(string sourceFilePath, string targetFolder, bool allowDuplicates)
		{
			if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("源文件不存在", sourceFilePath);

			Directory.CreateDirectory(targetFolder);

			string fileName = Path.GetFileName(sourceFilePath);
			string destPath = PathUtils.UnixPathCombine(targetFolder, fileName);
			if (allowDuplicates) destPath = DeduplicateFilePath(destPath);

			File.Copy(sourceFilePath, destPath, true);
			return destPath;
		}

		public static bool DeleteFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return false;
			if (!IsPersistentFile(filePath)) return false;

			File.Delete(filePath);
			return true;
		}
	}
}