namespace DingFrame.Module.UUI
{
	using UnityEngine;
	using TMPro;
	using DingFrame.Module.UI;

	public partial class DTMPText : TextMeshProUGUI, ILanguageUI
	{
		[SerializeField] private bool localization = false;
		[SerializeField] private int textHash = 0;
		public bool Localization => localization;
		public int TextHash => textHash;

		// todo: 切换语言的时候，需要替换字体，然后重新刷新 Text
	}
}