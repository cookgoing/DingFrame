namespace DingFrame.Utils
{
	using System.IO;

	public static class PathUtils
	{
		public static string ToWindowsPath(string path)
		{
			path = path.Replace("/", "\\");

			if (Directory.Exists(path) && !path.EndsWith("\\")) path += "\\";

			return path;
		}
		public static string ToUnixPath(string path)
		{
			path = path.Replace("\\", "/");
			if (Directory.Exists(path) && !path.EndsWith("/")) path += "/";

			return path;
		}
		
		public static string UnixPathCombine(params string[] paths) => ToUnixPath(Path.Combine(paths));
	
		public static string GetLastDirName(string path)
		{
			if (path.EndsWith("/") || path.EndsWith("\\")) return new DirectoryInfo(path).Name;
			else return new DirectoryInfo(Path.GetDirectoryName(path)).Name;
		}
	}
}