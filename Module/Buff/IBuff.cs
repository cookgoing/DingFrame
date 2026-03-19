namespace DingFrame.Module.Buff
{
	public interface IBuff
	{
		public enum State
		{
			InEffect,
			Pause,
			Over,
		}

		int BuffId { get; }
		IBuffTarget Target { get; }

		State GetState();
		void OnApply();
		void OnRemove();
		void OnUpdate(float dt);
		void Do();
	}

	public interface IBuff<T> : IBuff
	{
		new IBuffTarget<T> Target { get; }
		bool Do(out T value);
	}
}
