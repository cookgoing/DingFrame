namespace DingFrame
{
	using System;

	public sealed class UIDGenerator : Singleton<UIDGenerator>
	{
		private Random random;
		private SnowflakeIdGenerator snowGenerator;

		public UIDGenerator()
		{
			random = new Random();
			snowGenerator = new SnowflakeIdGenerator(DateTime.UtcNow.Ticks);
		}

		public static long GenerateLongID()
		{
			long timestamp = DateTime.UtcNow.Ticks;
			int random = Instance.random.Next(0, 9999);
			return long.Parse($"{timestamp}{random}");
		}

		public static long GenerateSnowID() => Instance.snowGenerator.NextId();

		public static string GenerateSimpleStrID() => Guid.NewGuid().ToString();
		public static string GenerateTimeStrID() => $"{DateTime.UtcNow.Ticks}{Guid.NewGuid().ToString("N")}";
		
	}
}