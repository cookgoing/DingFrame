namespace DingFrame.Module.TKUI
{
	using UnityEngine.UIElements;

	public interface IWidget
	{
		VisualElement Root{get;}

		void OnAttachElement(VisualElement root, params object[] args);
		void OnDetachElement();
	}
}