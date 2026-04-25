namespace DingFrame.Module.Time
{
	public struct GameTimer
	{
		public double TimerSecD{get; private set;} //s
		public float Scale{get; private set;}
		public float Delt{get; private set;}//s
		public readonly long TimerSecL => (long)TimerSecD;
		public readonly long TimerMilliSec => (long)(TimerSecD * 1000);

		public GameTimer(double timerSecD, float scale)
		{
			TimerSecD = timerSecD;
			Scale = scale;
			Delt = 0;
		}

		public void Update(float dt)
		{
			Delt = dt * Scale;
			TimerSecD += Delt;
		}

		public void SetTimeScale(float scale) => Scale = scale;
	}
}