namespace DingFrame.GameLaunch
{
	using System;
	using UnityEngine;
	using DingFrame;
	using DingFrame.Module;
	using DingFrame.Module.Time;

	public sealed class GameLauncher : MonoBehaviour
	{
		public ScriptableObject BusinessEntrance;
		private TimeModule timeModule;

		private void Awake()
		{
			Application.logMessageReceivedThreaded += LogMessageReceivedThreaded;
			Application.wantsToQuit += ApplicationWantsToQuit;
			LogRecorder.ClearExpiredLog();

			DontDestroyOnLoad(this);
			tag = FrameConfigure.GAME_LAUNCH_TAG;

			ModuleCollector.CreateInstance();
			UpdateCollector.CreateInstance();
			GameStateListenerCollector.CreateInstance();

			if (BusinessEntrance == null || BusinessEntrance is not ILauncherListener businessLancher) throw new Exception("[GameManager] no BusinessEntrance. the game can not running.");

			timeModule = new TimeModule();
			timeModule.SyncCenterTime(DateTime.UtcNow);
			ModuleCollector.Instance.AddModule(timeModule);

			businessLancher.OnGameLaunch();
			ModuleCollector.Instance.ForEach(m => m.Init());
		}
		private void Start() => GameStateListenerCollector.Instance.ForEach(listener => listener.GameEnter());
		private void OnDestroy()
		{
			ModuleCollector.Instance.ForEach(m => m.Dispose());
			Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
		}


		private void FixedUpdate() => UpdateCollector.Instance.ForEach(updater => updater.DFixedUpdate(Time.fixedDeltaTime * timeModule.GameTimer.Scale));
		private void Update()
		{
			UpdateCollector.Instance.ForEach(updater => updater.DUpdate(Time.deltaTime * timeModule.GameTimer.Scale));
			DLogger.DoLog();
		} 
		private void LateUpdate() => UpdateCollector.Instance.ForEach(updater => updater.DLateUpdate(Time.deltaTime * timeModule.GameTimer.Scale));


		private void OnApplicationPause(bool pauseStatus)
		{
		#if UNITY_ANDROID || UNITY_IOS
			if (pauseStatus) GameStateListenerCollector.Instance.ForEach(listener => listener.GameEnterBackground());
			else GameStateListenerCollector.Instance.ForEach(listener => listener.GameBackForeground());
		#endif
		}
		private void OnApplicationFocus(bool hasFocus)
		{
		#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
			if (hasFocus) GameStateListenerCollector.Instance.ForEach(listener => listener.GameBackForeground());
			else GameStateListenerCollector.Instance.ForEach(listener => listener.GameEnterBackground());
		#endif
		}
		private void OnApplicationQuit()
		{
			GameStateListenerCollector.Instance.ForEach(listener => listener.GameQuit());
			LogRecorder.OnAppExit();
			Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
		}


		private void LogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Log) return;

			LogRecorder.RecordLog($"{condition}\n{stackTrace}", type);
		}

		private bool ApplicationWantsToQuit() => GameStateListenerCollector.Instance.AppWantQuit();
	}
}