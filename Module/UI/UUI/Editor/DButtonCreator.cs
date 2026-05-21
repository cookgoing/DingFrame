namespace DingFrame.Module.UUI
{
	using System;
	using UnityEditor;
	using UnityEngine;

	public static class DButtonCreator
	{
		[MenuItem("GameObject/DUI/Button", false, 30)]
		public static void CreateDButton(MenuCommand menuCommand)
		{
			string path = "Assets/OuterRes/CustomUI/DButton.prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null) throw new Exception($"[CreateDButton]. no prefab in this path: {path}");

			GameObject buttonObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			GameObjectUtility.SetParentAndAlign(buttonObj, menuCommand.context as GameObject);

			if (buttonObj.transform.parent == null)
			{
				Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
				if (canvas != null) buttonObj.transform.SetParent(canvas.transform, false);
				else throw new Exception("[CreateDButton]. please create canvas firstly");
			}

            PrefabUtility.UnpackPrefabInstance(buttonObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);

			Undo.RegisterCreatedObjectUndo(buttonObj, "Create DButton");
			Selection.activeGameObject = buttonObj;
		}
	}
}