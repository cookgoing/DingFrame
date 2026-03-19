namespace DingFrame
{
	public interface IGameStateListener
	{
		Order.Order ListenOrder { get; }

		virtual bool AppWantsToQuit() => true;

		virtual void GameEnter() { }
		virtual void GameQuit() { }

		virtual void GameEnterBackground() { }
		virtual void GameBackForeground() { }
	}
}