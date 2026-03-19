namespace DingFrame
{
	using System;
	using System.Collections.Generic;

	public class GameStateComparer : IComparer<IGameStateListener>
	{
		public int Compare(IGameStateListener x, IGameStateListener y)
		{
			if (x.ListenOrder.MainOrder != y.ListenOrder.MainOrder) return x.ListenOrder.MainOrder - y.ListenOrder.MainOrder;
			return x.ListenOrder.SubOrder - y.ListenOrder.SubOrder;
		}
	}

	public sealed class GameStateListenerCollector : Singleton<GameStateListenerCollector>
	{
		internal SortedSet<IGameStateListener> GameStateListeners { get; private set; }

		public GameStateListenerCollector() => GameStateListeners = new SortedSet<IGameStateListener>(new GameStateComparer());


		public void AddGameStateListener(IGameStateListener updater) => GameStateListeners.Add(updater);
		public bool RemoveGameStateListener(IGameStateListener updater) => GameStateListeners.Remove(updater);


		public void ForEach(Action<IGameStateListener> action)
		{
			if (action == null)
			{
				DLogger.Error("action == null", "GameStateListenerCollector");
				return;
			}

			foreach (IGameStateListener listener in GameStateListeners) action(listener);
		}

		public bool AppWantQuit()
		{
			bool result = true;
			foreach (IGameStateListener listener in GameStateListeners) result &= listener.AppWantsToQuit();

			return result;
		}
	}
}