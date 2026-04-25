namespace DingFrame.Utils
{
	using System;
	using System.Text;
	using System.Security.Cryptography;

	public static class MiscUtils
	{
		private static Random _random;
		public static Random Random => _random ??= new Random();

		public static string ExceptionStr(Exception ex)
		{
			void Do(StringBuilder sb, Exception e)
			{
				sb.AppendLine($"{e.Message}\n{e.StackTrace}");

				if (e.InnerException != null)
				{
					sb.AppendLine("Inner Exception:");
					Do(sb, e.InnerException);
				}
			}

			StringBuilder sb = new();
			Do(sb, ex);

			return sb.ToString();
		}

		public static int? GetRandomInWeight(uint[] weights)
		{
			if (weights == null || weights.Length == 0) return null;

			long[] prefixSums = new long[weights.Length];
			prefixSums[0] = weights[0];
			for (int i = 1; i < weights.Length; i++)
			{
				prefixSums[i] = prefixSums[i - 1] + weights[i];
			}
			long totalWeight = prefixSums[^1];

			long randValue = RandomLong(prefixSums[0], totalWeight);
			int index = Array.BinarySearch(prefixSums, randValue);
			if (index < 0) index = ~index;

			return index;
		}

		public static uint RandomUint(uint min, uint max, bool inculeMax = false)
		{
			if (max < min) throw new Exception($"[MiscUtils.RandomUint]. max[{max}] < min[{min}]");

			return (uint)RandomLong(min, max, inculeMax);
		}

		public static long RandomLong(long min, long max, bool inculeMax = false)
		{
			if (max < min) throw new Exception($"[MiscUtils.RandomLong]. max[{max}] < min[{min}]");
			if (min == max) return min;

			ulong range = (ulong)(max - min);
			if (inculeMax && range < long.MaxValue) range++;
			ulong limit = ulong.MaxValue - (ulong.MaxValue % range);
			byte[] buffer = new byte[8];

			while (true)
			{
				RandomNumberGenerator.Fill(buffer);
				ulong randomValue = BitConverter.ToUInt64(buffer, 0);

				if (randomValue <= limit)
				{
					return (long)(randomValue % range) + min;
				}
			}
		}

		public static double RandomDouble(double min, double max)
		{
			if (max < min) throw new Exception($"[MiscUtils.RandomDouble]. max[{max}] < min[{min}]");

			double range = max - min;
			byte[] buffer = new byte[8];
			RandomNumberGenerator.Fill(buffer);
			ulong randomValue = BitConverter.ToUInt64(buffer, 0);

			return randomValue / (double)ulong.MaxValue * range + min;
		}

		public static string FormatNumber(long number)
		{
			if (number < 1_000) return number.ToString();
			if (number < 1_000_000) return (number / 1_000f).ToString("0.#") + "K";
			if (number < 1_000_000_000) return (number / 1_000_000f).ToString("0.#") + "M";
			if (number < 1_000_000_000_000) return (number / 1_000_000_000f).ToString("0.#") + "B";
			return (number / 1_000_000_000_000f).ToString("0.#") + "T";
		}

		public static string FormatByte(ulong _byte)
		{
			const double KB = 1024;
			const double MB = KB * 1024;
			const double GB = MB * 1024;
			
			if (_byte >= GB) return (_byte / GB).ToString("0.##") + "G";
			else if (_byte >= MB) return (_byte / MB).ToString("0.##") + "M";
			else if (_byte >= KB) return (_byte / KB).ToString("0.##") + "K";
			else return _byte.ToString("0.##") + "B";
		}
	
		public static string FormatTimeHMS(int sec, char hunit = ':', char munit = ':', char? sunit = null)
		{
			int h = sec / 3600;
			int m = sec % 3600 / 60;
			int s = sec % 60;
			StringBuilder sb = new();
			if (h > 0) sb.Append(h).Append(hunit);
			sb.Append(m).Append(munit);
			sb.Append(s.ToString("00")).Append(sunit);
			return sb.ToString();
		}
	
		public static float SafeDivide(float a, float b, float NaNValue = 0) => Math.Abs(b) < 0.0001f ? NaNValue : a / b;
	}
}