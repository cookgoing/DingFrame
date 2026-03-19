namespace DingFrame.Module.TKUI
{
	using UnityEngine;
	using UnityEngine.UIElements;
	using DingFrame.Module.Localization;

	[UxmlElement]
	public partial class DButton : Button, ILanguageTKUI
	{
		[UxmlAttribute("Localization")] public bool Localization { get; private set; } = true;
		[UxmlAttribute("TextHash")] public int TextHash { get; private set; } = 0;
		[UxmlAttribute("AutoSize")] public bool AutoSize { get; private set; } = false;

		private float initialFontSize;

		public DButton() : base()
		{
			if (Application.isPlaying)
			{
				RegisterCallbackOnce<AttachToPanelEvent>(e => ((ILanguageTKUI)this).OnUIAttachToPanel(e));
				RegisterCallbackOnce<GeometryChangedEvent>(e => AutoChangeFontSize());
				this.RegisterValueChangedCallback(OnTextValueChanged);
			}
		}

		void ILanguageTKUI.OnUIAttachToPanel(AttachToPanelEvent e)
		{
			if (Localization && TextHash != 0) text = LocalizationModule.GetStr(TextHash);

			initialFontSize = resolvedStyle.fontSize;
		}


		private void OnTextValueChanged(ChangeEvent<string> evt) => AutoChangeFontSize();

		public void AutoChangeFontSize()
		{
			if (!AutoSize) return;
			if (resolvedStyle.whiteSpace != WhiteSpace.NoWrap) return;
			if (contentRect.size == Vector2.zero) return;
			if (resolvedStyle.fontSize <= 1) return;

			string afterTrim = text.Trim();
			if (string.IsNullOrEmpty(afterTrim)) return;

			float labelContentSizeX = contentRect.size.x;
			float labelContentSizeY = contentRect.size.y;

			Vector2 measureTextSize = CalculateMeasureTextSize(labelContentSizeX, labelContentSizeY);
			while (measureTextSize.x > labelContentSizeX)
			{
				style.fontSize = resolvedStyle.fontSize - 1;
				measureTextSize = CalculateMeasureTextSize(labelContentSizeX, labelContentSizeY);
			}
			while (measureTextSize.x < labelContentSizeX && resolvedStyle.fontSize < initialFontSize)
			{
				style.fontSize = resolvedStyle.fontSize + 1;
				measureTextSize = CalculateMeasureTextSize(labelContentSizeX, labelContentSizeY);
			}
		}
		private Vector2 CalculateMeasureTextSize(float labelContentSizeX, float labelContentSizeY) => MeasureTextSize(text, labelContentSizeX, MeasureMode.Undefined, labelContentSizeY, MeasureMode.Undefined);
	}
}