namespace DingFrame
{
	using System;
	using System.Collections.Generic;

	public class UpdateComparer : IComparer<IUpdateCaller>
	{
		public int Compare(IUpdateCaller x, IUpdateCaller y)
		{
			if (x.UpdateOrder.MainOrder != y.UpdateOrder.MainOrder) return x.UpdateOrder.MainOrder - y.UpdateOrder.MainOrder;
			return x.UpdateOrder.SubOrder - y.UpdateOrder.SubOrder;
		}
	}

	public sealed class UpdateCollector : Singleton<UpdateCollector>
	{
		internal SortedSet<IUpdateCaller> UpdaterCallers { get; private set; }

		public UpdateCollector() => UpdaterCallers = new SortedSet<IUpdateCaller>(new UpdateComparer());

		public void AddUpdater(IUpdateCaller updater) => UpdaterCallers.Add(updater);
		public bool RemoveUpdater(IUpdateCaller updater) => UpdaterCallers.Remove(updater);

		public void ForEach(Action<IUpdateCaller> action)
		{
			if (action == null)
			{
				DLogger.Error("action == null", "UpdateCollector");
				return;
			}

			foreach (IUpdateCaller caller in UpdaterCallers) action(caller);
		}
	}
}