namespace DingFrame.Module.Buff
{
	public interface IBuffTarget
	{
		Order.Order BuffOrder { get; }
	}

	public interface IBuffTarget<T> : IBuffTarget
	{
		bool GetBuffBaseValue(int buffId, out T baseValue);
	}
}
