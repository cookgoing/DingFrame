namespace DingFrame.Module.TKUI
{
	using System;
	using UnityEngine;
	using UnityEngine.UIElements;
	using DingFrame.Module.Localization;
	using System.Threading.Tasks;

	[UxmlElement]
    public partial class DTextField : TextField, ILanguageTKUI
    {
		public static Func<string, string> GlobalTextReplaceFuc;
		public Func<string, string> TextReplaceFuc{get; private set;}

		private readonly Label placeholderLabel;
		public Action<string> OnValueChangedHandler;
		public override string value
        {
            get => base.value;
            set
            {
                base.value = value;
                UpdatePlaceholderVisibility();
            }
        }

		[UxmlAttribute("Localization")] public bool Localization { get; private set; } = true;
		[UxmlAttribute("TextHash")] public int TextHash { get; private set; } = 0;
		[UxmlAttribute("Placeholder")] public string Placeholder
        {
            get => placeholderLabel.text;
            set => placeholderLabel.text = value;
        }

		public DTextField()
		{
			placeholderLabel = new ();
			placeholderLabel.name = "placeholderLabel";
			placeholderLabel.pickingMode = PickingMode.Ignore;
			placeholderLabel.style.position = Position.Absolute;
			placeholderLabel.style.alignSelf = Align.Center;
			Add(placeholderLabel);

			RegisterCallback<FocusInEvent>(e => ShowHidePlaceholder(false));
			RegisterCallback<FocusOutEvent>(e => ShowHidePlaceholder(string.IsNullOrEmpty(value)));
			RegisterCallback<FocusOutEvent>(OnTextCheck);
			this.RegisterValueChangedCallback(e => OnValueChangedHandler?.Invoke(e.newValue));

			if (Application.isPlaying)
				RegisterCallbackOnce<AttachToPanelEvent>(e => ((ILanguageTKUI)this).OnUIAttachToPanel(e));
		}

		void ILanguageTKUI.OnUIAttachToPanel(AttachToPanelEvent e)
		{
			if (Localization && TextHash != 0) Placeholder = LocalizationModule.GetStr(TextHash);
		}

		public override void SetValueWithoutNotify(string newValue)
		{
			base.SetValueWithoutNotify(newValue);
			UpdatePlaceholderVisibility();
		}

		public void SetTextReplaceFuc(Func<string, string> textReplaceFuc) => TextReplaceFuc = textReplaceFuc;

		private void UpdatePlaceholderVisibility() => ShowHidePlaceholder(string.IsNullOrEmpty(value));

		private void ShowHidePlaceholder(bool isShow)
		{
			if (placeholderLabel == null) return;

			placeholderLabel.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
		}
    
		private void OnTextCheck(FocusOutEvent outE)
		{
			Func<string, string> textReplaceFuc = TextReplaceFuc ?? GlobalTextReplaceFuc;
			if (textReplaceFuc == null) return;

			SetValueWithoutNotify(textReplaceFuc.Invoke(value));
		}
	}
}
