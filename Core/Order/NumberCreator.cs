namespace DingFrame.Order
{
	using System.Collections.Generic;

	public class NumberCreator
	{
		private static NumberCreator _default;
		public static NumberCreator Default => _default ??= new NumberCreator();

		private SortedSet<int> ownedNumbers = new SortedSet<int>();

		public int GetNumber(bool add = false)
		{
			int result = ownedNumbers.Max + 1;
			int min = ownedNumbers.Min;
			if (min > 1) 
			{
				result = 1;
				goto Return;
			}

			int? preNumber = null;
			foreach(int number in ownedNumbers)
			{
				if (preNumber.HasValue && number > preNumber.Value + 1) 
				{
					result = preNumber.Value + 1;
					goto Return;
				}

				preNumber = number;
			}
			
		Return:
			if (add) AddNumber(result);
			return result;
		}

		public bool AddNumber(int number) => ownedNumbers.Add(number);
		public bool RemoveNumber(int number) => ownedNumbers.Remove(number);
	}
}