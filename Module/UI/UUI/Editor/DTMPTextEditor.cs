namespace DingFrame.Module.UUI
{
	using UnityEditor;
	using TMPro.EditorUtilities;

	[CustomEditor(typeof(DTMPText))]
	public class DTMPTextEditor : TMP_EditorPanelUI
	{
		SerializedProperty localization;
		SerializedProperty textHash;

		protected override void OnEnable()
		{
			base.OnEnable();

			localization = serializedObject.FindProperty("localization");
			textHash = serializedObject.FindProperty("textHash");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Localization", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(localization);
			EditorGUILayout.PropertyField(textHash);

			serializedObject.ApplyModifiedProperties();
		}
	}
}