namespace DingFrame.Utils
{
	using System;
	using System.Collections.Generic;

	public static class ArrayUtils
	{
		public static T[] ResizeArray<T>(T[] array, int newLen)
		{
			T[] newArray = new T[newLen];
			Array.Copy(array, newArray, Math.Min(array.Length, newLen));
			return newArray;
		}

		public static T FindClosestValue<T>(List<T> list, T target, T invalidValue) where T : IComparable<T>
		{
			if (list == null || list.Count == 0) return invalidValue;

			list.Sort();

			int left = 0;
			int right = list.Count - 1;
			int possibleIdx = -1;

			while (left <= right)
			{
				int mid = (left + right) / 2;
				if (list[mid].CompareTo(target) == 0)
				{
					return list[mid];
				}
				else if (list[mid].CompareTo(target) < 0)
				{
					possibleIdx = mid;
					left = mid + 1;
				}
				else
				{
					right = mid - 1;
				}
			}

			return possibleIdx >= 0 ? list[possibleIdx] : invalidValue;
		}
	}
}