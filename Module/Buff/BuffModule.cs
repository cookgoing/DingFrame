namespace DingFrame.Module.Buff
{
	using System.Collections.Generic;
	using DingFrame.Order;
	using DingFrame.Module.Time;

	public class BuffComparer : IComparer<IBuff>
	{
		public int Compare(IBuff x, IBuff y)
		{
			if (x.BuffId != y.BuffId) return x.BuffId - y.BuffId;
			if (x.Target == null) return -1;
			if (y.Target == null) return 1;
			if (x.Target.BuffOrder.MainOrder != y.Target.BuffOrder.MainOrder) return x.Target.BuffOrder.MainOrder - y.Target.BuffOrder.MainOrder;
			return x.Target.BuffOrder.SubOrder - y.Target.BuffOrder.SubOrder;
		}
	}

	public sealed class BuffModule : IModule, IUpdateCaller
	{
		private Order updateOrder = Order.CreateOrder(FrameConfigure.UPDATER_DEFAULT_ORDER);
		public Order UpdateOrder => updateOrder;
		private BuffComparer buffComparer;
		private Dictionary<int, SortedSet<IBuff>> dic;
		private TimeModule _timeModule;
		private TimeModule timeModule => _timeModule ?? ModuleCollector.GetModule<TimeModule>();

		public void Init()
		{
			buffComparer = new();
			dic = new();

			UpdateCollector.Instance.AddUpdater(this);
		}

		public void Dispose()
		{
			UpdateCollector.Instance.RemoveUpdater(this);
			
			foreach (var kv in dic)
				foreach (IBuff buff in kv.Value)
					buff.OnRemove();

			dic.Clear();
		}

		public void DUpdate(float dt)
		{
			foreach (var kv in dic)
			{
				HashSet<IBuff> willRemove = new();
				foreach (IBuff buff in kv.Value)
				{
					IBuff.State state = buff.GetState();
					if (state == IBuff.State.Over)
					{
						willRemove.Add(buff);
						continue;
					}

					if (buff.GetState() != IBuff.State.InEffect) continue;

					buff.OnUpdate(dt);
				}

				foreach (IBuff buff in willRemove) RemoveBuff(kv.Key, buff.Target);
			}
		}

		public bool AddBuff(int buffId, IBuff buff)
		{
			if (!dic.TryGetValue(buffId, out SortedSet<IBuff> sortedSet))
			{
				sortedSet = new(buffComparer);
				dic.Add(buffId, sortedSet);
			}

			bool result = sortedSet.Add(buff);
			if (result) buff.OnApply();
			return result;
		}

		public bool RemoveBuff(int buffId, IBuffTarget target)
		{
			if (!dic.TryGetValue(buffId, out SortedSet<IBuff> sortedSet)) return false;

			return sortedSet.RemoveWhere(buff =>
			{
				buff.OnRemove();
				return buff.Target == target;
			}) > 0;
		}

		public IBuff GetBuff(int buffId, IBuffTarget target)
		{
			if (!dic.TryGetValue(buffId, out SortedSet<IBuff> sortedSet)) return null;

			foreach (IBuff buff in sortedSet)
			{
				if (buff.Target != target) continue;

				return buff;
			}

			return null;
		}
		public T GetBuff<T>(int buffId, IBuffTarget target) where T : class, IBuff => GetBuff(buffId, target) as T;

		public void DoBuff(int buffId, IBuffTarget target) => GetBuff(buffId, target)?.Do();
		public bool DoBuff<T>(int buffId, IBuffTarget target, out T value)
		{
			value = default;
			IBuff buff = GetBuff(buffId, target);
			if (buff is not IBuff<T> buffT)
			{
				if (target is IBuffTarget<T> targetT && targetT.GetBuffBaseValue(buffId, out T baseValue)) value = baseValue;

				return false;
			}

			return buffT.Do(out value);
		}
	}
}