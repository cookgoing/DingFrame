namespace DingFrame.Utils
{
	using System;
	using System.Text;
	using UnityEngine;
	using UnityEngine.UIElements;

	public static class DeviceInfo
	{
		public static bool IsInEditor
			=> Application.platform == RuntimePlatform.LinuxEditor
			|| Application.platform == RuntimePlatform.WindowsEditor
			|| Application.platform == RuntimePlatform.OSXEditor;

		public static bool IsInDestop
			=> Application.platform == RuntimePlatform.OSXPlayer
			|| Application.platform == RuntimePlatform.WindowsPlayer
			|| Application.platform == RuntimePlatform.LinuxPlayer;

		public static bool IsInMobile
			=> Application.platform == RuntimePlatform.IPhonePlayer
			|| Application.platform == RuntimePlatform.Android;
	
		public static bool IsPortrait => IsInMobile ? Screen.orientation == ScreenOrientation.Portrait : Screen.height >= Screen.width;

		public static string GetSystemInfo()
		{
			StringBuilder systemInfo = new();
			systemInfo.AppendLine("*********************************************************************************************************start");
			systemInfo.AppendLine("By: " + SystemInfo.deviceName);
			DateTime now = DateTime.Now;
			systemInfo.AppendLine(string.Concat(new object[] { now.Year.ToString(), "年", now.Month.ToString(), "月", now.Day, "日  ", now.Hour.ToString(), ":", now.Minute.ToString(), ":", now.Second.ToString() }));
			systemInfo.AppendLine();
			systemInfo.AppendLine("操作系统:  " + SystemInfo.operatingSystem);
			systemInfo.AppendLine("系统内存大小:  " + SystemInfo.systemMemorySize);
			systemInfo.AppendLine("设备模型:  " + SystemInfo.deviceModel);
			systemInfo.AppendLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
			systemInfo.AppendLine("处理器数量:  " + SystemInfo.processorCount);
			systemInfo.AppendLine("处理器类型:  " + SystemInfo.processorType);
			systemInfo.AppendLine("显卡UID:  " + SystemInfo.graphicsDeviceID);
			systemInfo.AppendLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
			systemInfo.AppendLine("显卡类型:  " + SystemInfo.graphicsDeviceType);
			systemInfo.AppendLine("显卡供应商UID:  " + SystemInfo.graphicsDeviceVendorID);
			systemInfo.AppendLine("显卡供应商:  " + SystemInfo.graphicsDeviceVendor);
			systemInfo.AppendLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
			systemInfo.AppendLine("显存大小:  " + SystemInfo.graphicsMemorySize);
			systemInfo.AppendLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
			systemInfo.AppendLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
			systemInfo.AppendLine("*********************************************************************************************************end");
			return systemInfo.ToString();
		}
	
		public static Vector2 GetDeviceUIRatios(PanelSettings panelSettings)
		{
			Vector2 referenceResolution = panelSettings.referenceResolution;
			int screenWidth = Screen.width;
			int screenHeight = Screen.height;
			float designWidth = referenceResolution.x;
			float designHeight = referenceResolution.y;
			return new Vector2(screenWidth / designWidth, screenHeight / designHeight);
		}
		public static float GetDeviceUIRatio(PanelSettings panelSettings)
		{
			Vector2 deviceUIRatio = GetDeviceUIRatios(panelSettings);
			return Math.Min(deviceUIRatio.x, deviceUIRatio.y);
		}
	}
}