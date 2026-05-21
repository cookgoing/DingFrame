namespace DingFrame.Module.UUI
{
	using UnityEngine;

	public interface IWidget
	{
		RectTransform Root{get;}

		void OnAttachElement(RectTransform root, params object[] args);
		void OnDetachElement();
	}
}