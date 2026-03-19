namespace DingFrame.Module.Event
{
	using System;
	using System.Collections.Generic;

	public sealed class EventModule : IModule
	{
		internal Dictionary<string, Action<object>> EventDic{get; private set;}

		public void Init() => EventDic = new(50);


		public void AddEvent(string key, Action<object> _ev)
		{
			if (EventDic.TryGetValue(key, out Action<object> ev)) ev += _ev;
			else ev = _ev;

			EventDic[key] = ev;
		}

		public bool RemoveEvent(string key, Action<object> _ev)
		{
			if (EventDic.TryGetValue(key, out Action<object> ev)) 
			{
				ev -= _ev;
				if (ev == null) EventDic.Remove(key);
				else EventDic[key] = ev;
				return true;
			}
			return false;
		}
	
		public bool ClearEvent(string key) => EventDic.Remove(key);

		public void Trigger(string key, object arg = null)
		{
			if (!EventDic.TryGetValue(key, out Action<object> ev)) return;

			ev(arg);
		}
	}
}