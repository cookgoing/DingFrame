namespace DingFrame.Module.ObjPool
{
	using System.Collections.Generic;

	public class ObjPoolManager : IModule
	{
		public HashSet<IObjPool> Pools { get; private set; }

		public void Init() => Pools = new();

		public void AddPool(IObjPool pool) => Pools.Add(pool);
		public bool RemovePool(IObjPool pool) => Pools.Remove(pool);

		public T GetPool<T>() where T : class, IObjPool
		{
			foreach(IObjPool objPool in Pools)
			{
				if (objPool == null || objPool is not T) continue;

				return objPool as T;
			}

			return null;
		}
	}
}