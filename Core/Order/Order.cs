namespace DingFrame.Order
{
	using System;

	public struct Order : IEquatable<Order>
	{
		public int MainOrder;
		public int SubOrder;

		public static Order CreateOrder(int mainOrder, int subOrder)
		{
			return new Order()
			{
				MainOrder = mainOrder,
				SubOrder = subOrder,
			};
		}
		public static Order CreateOrder(int mainOrder)
		{
			return new Order()
			{
				MainOrder = mainOrder,
				SubOrder = NumberCreator.Default.GetNumber(true),
			};
		}

		public readonly override bool Equals(object obj) => obj is Order other && Equals(other);
		public readonly bool Equals(Order other) => MainOrder == other.MainOrder && SubOrder == other.SubOrder;
		public readonly override int GetHashCode() => HashCode.Combine(MainOrder, SubOrder);
		public static bool operator ==(Order left, Order right) => left.Equals(right);
    	public static bool operator !=(Order left, Order right) => !left.Equals(right);

		public override string ToString() => $"Order({MainOrder}, {SubOrder})";
	}
}