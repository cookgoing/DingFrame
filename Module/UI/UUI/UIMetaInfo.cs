namespace DingFrame.Module.UUI
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
		public string Path;
		public int Order;
		public bool IsFullScreen;
		public string BackgroundType;
	}
}
