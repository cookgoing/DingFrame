namespace DingFrame.GameLaunch
{
	using System;
	using UnityEngine;
	using DingFrame;
	using DingFrame.Module;

	public sealed class GameLauncher : MonoBehaviour
	{
		private bool isInitPause = true, isInitFocus = true;
		public ScriptableObject BusinessEntrance;


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

			businessLancher.OnGameLaunch();
			ModuleCollector.Instance.ForEach(m => m.Init());
		}
		private void Start() => GameStateListenerCollector.Instance.ForEach(listener => listener.GameEnter());
		private void OnDestroy()
		{
			ModuleCollector.Instance.ForEach(m => m.Dispose());
			Application.logMessageReceivedThreaded -= LogMessageReceivedThreaded;
		}


		private void FixedUpdate() => UpdateCollector.Instance.ForEach(updater => updater.DFixedUpdate(Time.fixedDeltaTime));
		private void Update()
		{
			UpdateCollector.Instance.ForEach(updater => updater.DUpdate(Time.deltaTime));
			DLogger.DoLog();
		} 
		private void LateUpdate() => UpdateCollector.Instance.ForEach(updater => updater.DLateUpdate(Time.deltaTime));


		private void OnApplicationPause(bool pauseStatus)
		{
			if (isInitPause)
			{
				isInitPause = false;
				return;
			}
			if (!pauseStatus) return;

			GameStateListenerCollector.Instance.ForEach(listener => listener.GameEnterBackground());
		}
		private void OnApplicationFocus(bool hasFocus)
		{
			if (isInitFocus)
			{
				isInitFocus = false;
				return;
			}
			if (!hasFocus) return;

			GameStateListenerCollector.Instance.ForEach(listener => listener.GameBackForeground());
		}
		private void OnApplicationQuit()
		{
			GameStateListenerCollector.Instance.ForEach(listener => listener.GameQuit());
			LogRecorder.OnAppExit();
		}


		private void LogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Log) return;

			LogRecorder.RecordLog($"{condition}\n{stackTrace}", type);
		}

		private bool ApplicationWantsToQuit() => GameStateListenerCollector.Instance.AppWantQuit();
	}
}