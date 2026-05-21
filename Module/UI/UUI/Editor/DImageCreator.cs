namespace DingFrame.Module.UUI
{
	using System;
	using UnityEditor;
	using UnityEngine;

	public static class DImageCreator
	{
		[MenuItem("GameObject/DUI/Image", false, 30)]
		public static void CreateDImage(MenuCommand menuCommand)
		{
			string path = "Assets/OuterRes/CustomUI/DImage.prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null) throw new Exception($"[CreateDImage]. no prefab in this path: {path}");

			GameObject imageObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			GameObjectUtility.SetParentAndAlign(imageObj, menuCommand.context as GameObject);

			if (imageObj.transform.parent == null)
			{
				Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
				if (canvas != null) imageObj.transform.SetParent(canvas.transform, false);
				else throw new Exception("[CreateDImage]. please create canvas firstly");
			}

			PrefabUtility.UnpackPrefabInstance(imageObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);

			Undo.RegisterCreatedObjectUndo(imageObj, "Create DImage");
			Selection.activeGameObject = imageObj;
		}
	}
}