namespace DingFrame.Module.Time
{
	using System;
	using DingFrame.Order;

	public class TimeModule : IModule, IGameStateListener, IUpdateCaller
	{
		private Order listenOrder = Order.CreateOrder(FrameConfigure.GAMESTATE_DEFAULT_ORDER);
		private Order updateOrder = Order.CreateOrder(FrameConfigure.UPDATER_DEFAULT_ORDER);
		public Order ListenOrder => listenOrder;
		public Order UpdateOrder => updateOrder;
		
		public bool IsGamePause {get; private set;}
		public GameTimer GameTimer {get; private set;}
		
		protected TimeSpan spanInCenterDevice;
		public long CenterTicks => (DateTime.UtcNow + spanInCenterDevice).Ticks;
		public long CenterSec => CenterTicks / TimeSpan.TicksPerSecond;
		public long CenterMilliSec => CenterTicks / TimeSpan.TicksPerMillisecond;

		public virtual void Init() => GameStateListenerCollector.Instance.AddGameStateListener(this);	
		public virtual void Dispose() => GameStateListenerCollector.Instance.RemoveGameStateListener(this);

		public virtual void GameEnter()
		{
			UpdateCollector.Instance.AddUpdater(this);

			GameTimer = new(0, 1);
		}
		public virtual void GameQuit() => UpdateCollector.Instance.RemoveUpdater(this);
		public virtual void GameEnterBackground() => IsGamePause = !UnityEngine.Application.runInBackground;
		public virtual void GameBackForeground() => IsGamePause = false;

		public void DUpdate(float dt)
		{
			if (IsGamePause)
			{
				GameTimer.Update(0);
				return;
			}

			GameTimer.Update(dt);
		}

		public void SyncCenterTime(DateTime centerDate)
		{
			DateTime deviceDate = DateTime.UtcNow;
			spanInCenterDevice = centerDate - deviceDate;
		}
	}
}