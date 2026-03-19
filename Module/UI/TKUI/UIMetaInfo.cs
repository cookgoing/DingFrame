namespace DingFrame.Module.TKUI
{
	public enum UILayer
	{
		MainView,
		Background,
		NormalView,
		Tip,
		Guide,
		Float,
	}
	
	public struct UIMetaInfo
	{
		public UILayer Layer;
		public string Type;	
		public string PanelSettingPath;
		public string UXMLPath;
		public bool IsFullScreen;
		public string BackgroundType;
	}
}
