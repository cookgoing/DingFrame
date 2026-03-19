namespace DingFrame
{
	using System;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Collections.Concurrent;
	using UnityEngine;
	using Utils;

	public static class LogRecorder
	{
		public const string LOG_FILE_NAME_FORMAT = "yyyy-MM-dd";
		public const string TIME_FORMAT = "HH:mm:ss.fff";
		public const int EXPIRED_MONTH = 1;

		private static readonly object logLock = new ();
		private static readonly ConcurrentDictionary<string, ConcurrentQueue<string>> logDic;
		private static readonly AutoResetEvent logEvent;
		private static CancellationTokenSource logRecordCancelSource;
		
		static LogRecorder()
		{
			logDic = new();
			logEvent = new (false);
			logRecordCancelSource = new();

			Task.Run(DoRecordLog, logRecordCancelSource.Token);
		} 

		public static void OnAppExit()
		{
			logDic.Clear();
			logRecordCancelSource.Cancel();
			logEvent.Dispose();
			logRecordCancelSource.Dispose();
		}

		public static void ClearExpiredLog()
		{
			string logDir = FrameConfigure.LogDir;

			Task.Run(() =>
			{
				lock(logLock)
				{
					try
					{  
						if (!Directory.Exists(logDir)) return;
						
						DirectoryInfo directory = new (logDir);
						DateTime now = DateTime.Now;
						foreach (FileInfo file in directory.GetFiles())
						{
							string fileName = Path.GetFileNameWithoutExtension(file.Name);
							if (!DateTime.TryParseExact(fileName, LOG_FILE_NAME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fileDate)) continue;

							DateTime maxDate = fileDate.AddMonths(EXPIRED_MONTH);
							bool isExpired = now > maxDate;
							if (isExpired)
							{
								file.IsReadOnly = false;
								file.Delete();
							}
							
						}
					}
					catch(Exception e)
					{
						string logStr = $"[LogRecorder][RecordSystemInfo]. {MiscUtils.ExceptionStr(e)}";
						DLogger.Warn(logStr);
						RecordLog(logStr, LogType.Exception);
					}
				}
			});
		}

		public static void RecordLog(string logStr, LogType type)
		{
			DateTime now = DateTime.Now;
			string fileName = now.ToString(LOG_FILE_NAME_FORMAT, CultureInfo.InvariantCulture);
			ConcurrentQueue<string> logQueue = logDic.GetOrAdd(fileName, new ConcurrentQueue<string>());
			StringBuilder sb = new();
			string time = DateTime.Now.ToString(TIME_FORMAT, CultureInfo.InvariantCulture);
			string logType = type.ToString();
			sb.Append('[').Append(time).Append(']').Append('[').Append(logType).AppendLine("]");
			sb.AppendLine(logStr);

			logQueue.Enqueue(sb.ToString());
			logEvent.Set();
		}

		private static void DoRecordLog()
		{
			try
			{
				while (true)
				{
					int index = WaitHandle.WaitAny(new WaitHandle[] { logEvent, logRecordCancelSource.Token.WaitHandle });
					if (index == 1) break;

					foreach(var kv in logDic)
					{
						string fileName = kv.Key;
						if (!Directory.Exists(FrameConfigure.LogDir)) Directory.CreateDirectory(FrameConfigure.LogDir);
						string logFilePath = PathUtils.UnixPathCombine(FrameConfigure.LogDir, fileName + ".txt");
						bool isNewFile = !File.Exists(logFilePath);
						FileAttributes attributes = FileAttributes.Archive | FileAttributes.Normal;
						if (!isNewFile)
						{
							attributes = File.GetAttributes(logFilePath);
							File.SetAttributes(logFilePath, attributes & ~FileAttributes.ReadOnly);
						}
						StreamWriter writer = new(logFilePath, true);

						while (kv.Value.TryDequeue(out string log)) writer.Write(log);
						
						writer.Close();

						File.SetAttributes(logFilePath, attributes);
					}
				}
			}
			catch(Exception e)
			{
				string logStr = $"[LogRecorder][DoRecordLog]. {MiscUtils.ExceptionStr(e)}";
				DLogger.Warn(logStr);
				RecordLog(logStr, LogType.Exception);
			}
		}
	}
}