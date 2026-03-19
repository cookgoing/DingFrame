namespace DingFrame.Module.TKUI
{
	using UnityEngine.UIElements;
	using DingFrame.Module.UI;

	public interface ILanguageTKUI : ILanguageUI
	{
		void OnUIAttachToPanel(AttachToPanelEvent e);
	}
}