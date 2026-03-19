namespace DingFrame.Module.TKUI
{
	public abstract class WidgetHandler
	{
		public IWidget Widget { get; private set; }

		public virtual void AttachWidget(IWidget widget) => Widget = widget;
		public virtual void DeattachWidget() => Widget = null;
	}

	public abstract class ViewHandler
	{
		public IView View { get; private set; }
		public IView BgView { get; private set; }

		public virtual void AttachView(IView view) => View = view;
		public virtual void OnViewOpen(){}
		public virtual void AttachBgView(IView bgView) => BgView = bgView;
	}
}