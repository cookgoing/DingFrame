using System.Runtime.InteropServices.WindowsRuntime;

namespace DingFrame.Module.ObjPool
{
	using System;
	using System.Threading.Tasks;
	using System.Collections.Generic;

	public interface IObjPoolable
	{
		void OnCreated();
		void OnWillDestroy();

		void OnSpawned();
    	void OnWillDespawn();
	}

	public interface IObjPool
	{
		int MaxCount {get; }

		object GetObj();
		bool BackObj(object obj);
		void Clean();
	}

	public abstract class ObjPool<T> : IObjPool where T : IObjPoolable
	{
		public virtual int MaxCount {get; }

		protected Queue<T> pool;
		protected HashSet<T> outer;
		protected Func<Task<T>> createFunc;

		public object GetObj() => Get().Result;
		public  bool BackObj(object obj) => Back((T)obj);
		public virtual void Clean()
		{
			while(pool.Count > 0) pool.Dequeue()?.OnWillDestroy();
			foreach (T obj in outer) obj.OnWillDestroy();

			pool.Clear();
			outer.Clear();
		}
		
		public virtual async Task<T> Get()
		{
			T obj;
			if (pool.Count > 0)
			{
				obj = pool.Dequeue();
				obj.OnSpawned();
				goto End;
			}
			
			obj = createFunc != null ? await createFunc() : default;
			obj?.OnCreated();

		End:
			outer.Add(obj);
			return obj;
		}
		public virtual bool Back(T obj)
		{
			if (obj == null) return false;
			if (pool.Count >= MaxCount)
			{
				obj.OnWillDestroy();
				outer.Remove(obj);
				return false;
			}

			obj.OnWillDespawn();
			pool.Enqueue(obj);
			outer.Remove(obj);
			return true;
		}
	}
}