namespace DingFrame
{
	using System;
	using System.Threading;
	using System.Collections.Concurrent;
	using UnityEngine;
	using Object = UnityEngine.Object;
	
	public static class DLogger
	{
		public enum LogLevel
		{
			Info = 1,
			Warn = 2,
			Error = 3,
			Exception = 4,
		}

		public struct LogItem
		{
			public string Message;
			public LogType Type;
			public string Tag;
			public Object Context;
			public int threadId;

			public string ToLogString() => $"[Thread-{threadId}] {Message}";
		}

		private static ILogger logger = Debug.unityLogger;
		public static LogLevel Level = LogLevel.Info;
		public static ConcurrentQueue<LogItem> queue = new();

		public static void Info(string message, string tag = "Ding", Object context = null)
		{
			if (Level > LogLevel.Info) return;

			logger.Log(LogType.Log, message, tag, context);
		}

		public static void Warn(string message, string tag = "Ding", Object context = null)
		{
			if (Level > LogLevel.Warn) return;

			logger.Log(LogType.Warning, message, tag, context);
		}

		public static void Error(string message, string tag = "Ding", Object context = null)
		{
			if (Level > LogLevel.Error) return;

			logger.Log(LogType.Error, message, tag, context);
		}

		public static void Exception(Exception e, string tag = "Ding", Object context = null)
		{
			if (Level > LogLevel.Exception) return;

			queue.Enqueue(new LogItem
			{
				Message = Utils.MiscUtils.ExceptionStr(e),
				Type = LogType.Exception,
				Tag = tag,
				Context = context,
				threadId = Thread.CurrentThread.ManagedThreadId
			});
		}

		public static void Record(string message)
		{
			Info(message);
			LogRecorder.RecordLog(message, LogType.Log);
		}

		public static void DoLog()
		{
			while (queue.TryDequeue(out LogItem item))
			{
				switch (item.Type)
				{
					case LogType.Log:
						logger.Log(LogType.Log, item.ToLogString(), item.Tag, item.Context);
						break;
					case LogType.Warning:
						logger.Log(LogType.Warning, item.ToLogString(), item.Tag, item.Context);
						break;
					case LogType.Error:
						logger.Log(LogType.Error, item.ToLogString(), item.Tag, item.Context);
						break;
					case LogType.Exception:
						logger.Log(LogType.Exception, item.ToLogString(), item.Tag, item.Context);
						break;
				}
			}
		}		
	}
}