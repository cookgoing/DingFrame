namespace DingFrame.Utils
{
	using System;
	using System.IO;
	using System.Diagnostics;

	public static class BatchUtils
	{
		public static string ExcuteProgramFile(string batchFilePath, string arguments, string workingDirectory)
		{
			if (!File.Exists(batchFilePath)) throw new FileNotFoundException($"[ExcuteBatchFile] 没有这个应用程序: {batchFilePath}", batchFilePath);

			return ExcuteCommand(batchFilePath, arguments, workingDirectory);
		}
		
		public static string ExcuteCommand(string cmd, string arguments, string workingDirectory)
		{
			ProcessStartInfo startInfo = new()
			{
				FileName = cmd,
				Arguments = arguments,
				WorkingDirectory = workingDirectory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			using Process process = new() { StartInfo = startInfo };

			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			process.WaitForExit();

			if (process.ExitCode != 0) throw new Exception($"【BatchUtils.ExcuteProgramFile】执行失败：{error}");

			return output;
		}
	}
}