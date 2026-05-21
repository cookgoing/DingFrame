namespace DingFrame.Module.UUI
{
	using System;
	using UnityEditor;
	using UnityEngine;

	public static class DInputFieldCreator
	{
		[MenuItem("GameObject/DUI/InputField", false, 30)]
		public static void CreateDInputField(MenuCommand menuCommand)
		{
			string path = "Assets/OuterRes/CustomUI/DInputField.prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null) throw new Exception($"[CreateDInputField]. no prefab in this path: {path}");

			GameObject inputFieldObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			GameObjectUtility.SetParentAndAlign(inputFieldObj, menuCommand.context as GameObject);

			if (inputFieldObj.transform.parent == null)
			{
				Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
				if (canvas != null) inputFieldObj.transform.SetParent(canvas.transform, false);
				else throw new Exception("[CreateDInputField]. please create canvas firstly");
			}

			PrefabUtility.UnpackPrefabInstance(inputFieldObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);

			Undo.RegisterCreatedObjectUndo(inputFieldObj, "Create DInputField");
			Selection.activeGameObject = inputFieldObj;
		}
	}
}