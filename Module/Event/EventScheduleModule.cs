namespace DingFrame.Module.Event
{
	using System;
	using System.Collections.Generic;
	using DingFrame.Order;
	using DingFrame.Module.Time;

	public class ScheduleComparer : IComparer<ScheduleInfo>
	{
		public int Compare(ScheduleInfo x, ScheduleInfo y)
		{
			if (x.ScheduleOrder.MainOrder != y.ScheduleOrder.MainOrder) return x.ScheduleOrder.MainOrder - y.ScheduleOrder.MainOrder;
			return x.ScheduleOrder.SubOrder - y.ScheduleOrder.SubOrder;
		}
	}

	public class ScheduleInfo
	{
		private Order scheduleOrder = Order.CreateOrder(FrameConfigure.SCHEDULE_DEFAULT_ORDER);
		public Order ScheduleOrder => scheduleOrder;
		private GameTimer timeSpace;
		private double nextExcuteSec;
		public float DelySec { get; private set; }
		public int LoopCount { get; private set; }
		public float IntervalSec { get; private set; }
		public Action Event { get; private set; }

		public bool IsInfiniteLoop => LoopCount == -1;

		public ScheduleInfo(GameTimer _timeSpace, float delySec, int loopCount, float intervalSec, Action ev)
		{
			timeSpace = _timeSpace;
			nextExcuteSec = _timeSpace.TimerSecD + delySec;
			DelySec = delySec;
			LoopCount = loopCount;
			IntervalSec = intervalSec;
			Event = ev;
		}

		public bool SetOrder(Order newOrder)
		{
			if (newOrder == scheduleOrder) return false;

			scheduleOrder = newOrder;
			return true;
		}

		// return: isFinished
		internal bool Excute()
		{
			if (LoopCount == 0) return true;
			if (timeSpace.TimerSecD < nextExcuteSec) return false;

			Event?.Invoke();

			nextExcuteSec = timeSpace.TimerSecD + IntervalSec;

			if (!IsInfiniteLoop) LoopCount--;
			return LoopCount == 0;
		}
	}

	public sealed class EventScheduleModule : IModule, IUpdateCaller
	{
		private Order updateOrder = Order.CreateOrder(FrameConfigure.UPDATER_DEFAULT_ORDER);
		public Order UpdateOrder => updateOrder;
		public SortedSet<ScheduleInfo> Schedules { get; private set; }

		public void Init() 
		{
			UpdateCollector.Instance.AddUpdater(this);
			Schedules = new(new ScheduleComparer());
		}
		public void Dispose() 
		{
			UpdateCollector.Instance.RemoveUpdater(this);
		}

		public void DUpdate(float dt)
		{
			if (Schedules == null || Schedules.Count == 0) return;

			Schedules.RemoveWhere(info => info.Excute());
		}

		public bool AddSchedule(ScheduleInfo info) => Schedules.Add(info);
		public bool RemoveSchedule(ScheduleInfo info)
		{
			if (info == null) return false;
			
			return Schedules.Remove(info);
		}

		public ScheduleInfo AddSchedule(Action ev, float delySec = 0, int loopCount = -1, float intervalSec = 0.02f, GameTimer? timeSpace = null)
		{
			timeSpace ??= ModuleCollector.GetModule<TimeModule>().GameTimer;

			ScheduleInfo info = new ScheduleInfo(timeSpace.Value, delySec, loopCount, intervalSec, ev);
			bool isAdd = AddSchedule(info);

			return isAdd ? info : null;
		}
		public int RemoveSchedule(Action ev) => Schedules.RemoveWhere(info => info.Event == ev);
	}
}