namespace DingFrame
{
	using UnityEngine;
	using Utils;

	public static class FrameConfigure
	{
		public const string GAME_LAUNCH_TAG = "GameController";
		public const string TOOL_KIT_ROOTUI_TAG = "TKUIRoot";
		public const string AUDIO_MODULE_NAME = "AudioModule";
		public const string AUDIO_TAG = "Audio";

		public const string LANGUAGE_KEY = "LANGUAGE_KEY";

		public const string DTextField_style_resPath = "CommonUI/Styles/DTextField";

		private static string logDir;
		public static void SetLogDir(string dirPath) => logDir = dirPath;
		public static string LogDir => logDir ?? PathUtils.UnixPathCombine(Application.persistentDataPath, "Log");

		public const int GAMESTATE_DEFAULT_ORDER = 10000;
		public const int UPDATER_DEFAULT_ORDER = 10000;
		public const int BUFF_DEFAULT_ORDER = 10000;
		public const int SCHEDULE_DEFAULT_ORDER = 10000;
	}
}