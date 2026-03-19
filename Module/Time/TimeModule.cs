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
		
		protected bool isGamePause;
		public double ClientSecD{get; private set;}
		public float Scale{get; private set;}
		public float Delt{get; private set;}//s
		public long ClientSecL => (long)ClientSecD;
		public long ClientMilliSec => (long)(ClientSecD * 1000);
		
		protected TimeSpan spanInCenterDevice;
		public long CenterTicks => (DateTime.UtcNow + spanInCenterDevice).Ticks;
		public long CenterSec => CenterTicks / TimeSpan.TicksPerSecond;
		public long CenterMilliSec => CenterTicks / TimeSpan.TicksPerMillisecond;

		public virtual void Init() => GameStateListenerCollector.Instance.AddGameStateListener(this);	
		public virtual void Dispose() => GameStateListenerCollector.Instance.RemoveGameStateListener(this);


		public virtual void GameEnter()
		{
			UpdateCollector.Instance.AddUpdater(this);

			ClientSecD = 0;
			SetTimeScale(1);
		}
		public virtual void GameQuit() => UpdateCollector.Instance.RemoveUpdater(this);
		public virtual void GameEnterBackground() => isGamePause = !UnityEngine.Application.runInBackground;
		public virtual void GameBackForeground() => isGamePause = false;


		public void DUpdate(float dt)
		{
			if (isGamePause)
			{
				Delt = 0;
				return;
			}

			Delt = dt * Scale;
			ClientSecD += Delt;
		}


		public void SetTimeScale(float scale) => Scale = scale;

		public void SyncCenterTime(DateTime centerDate)
		{
			DateTime deviceDate = DateTime.UtcNow;
			spanInCenterDevice = centerDate - deviceDate;
		}
	}
}