namespace DingFrame
{
	using System;
	public class SnowflakeIdGenerator
	{
		private static readonly object _lock = new object();
		private const long Twepoch = 687888001020L; // 起始时间戳
		private const int WorkerIdBits = 10; // 节点 ID 位数
		private const int SequenceBits = 12; // 序列号位数

		private const long MaxWorkerId = 1L << WorkerIdBits;
		private const long MaxSequence = 1L << SequenceBits;

		private readonly long _workerId;
		private long _sequence = 0L;
		private long _lastTimestamp = -1L;

		public SnowflakeIdGenerator(long workerId) => _workerId = workerId & (MaxWorkerId - 1);

		public long NextId()
		{
			lock (_lock)
			{
				long timestamp = GetCurrentTimestamp();

				if (timestamp < _lastTimestamp)
				{
					throw new InvalidOperationException("Clock moved backwards.");
				}

				if (timestamp == _lastTimestamp)
				{
					_sequence = (_sequence + 1) & MaxSequence;
					if (_sequence == 0)
					{
						timestamp = WaitNextMillis(_lastTimestamp);
					}
				}
				else
				{
					_sequence = 0;
				}

				_lastTimestamp = timestamp;

				return ((timestamp - Twepoch) << (WorkerIdBits + SequenceBits))
					| (_workerId << SequenceBits)
					| _sequence;
			}
		}

		private long GetCurrentTimestamp()
		{
			return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		}

		private long WaitNextMillis(long lastTimestamp)
		{
			long timestamp = GetCurrentTimestamp();
			while (timestamp <= lastTimestamp)
			{
				timestamp = GetCurrentTimestamp();
			}
			return timestamp;
		}
	}

}