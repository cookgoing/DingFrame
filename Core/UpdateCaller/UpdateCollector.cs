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
		private readonly List<IUpdateCaller> _cache;

		public UpdateCollector()
		{
			UpdaterCallers = new SortedSet<IUpdateCaller>(new UpdateComparer());
			_cache = new (128);
		}

		public void AddUpdater(IUpdateCaller updater) => UpdaterCallers.Add(updater);
		public bool RemoveUpdater(IUpdateCaller updater) => UpdaterCallers.Remove(updater);

		public void ForEach(Action<IUpdateCaller> action)
		{
			if (action == null)
			{
				DLogger.Error("action == null", "UpdateCollector");
				return;
			}

			_cache.Clear();
    		_cache.AddRange(UpdaterCallers);
			foreach (IUpdateCaller caller in _cache) action(caller);
		}
	}
}