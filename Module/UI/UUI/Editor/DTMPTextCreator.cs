namespace DingFrame.Module.UUI
{
	using System;
	using UnityEditor;
	using UnityEngine;

	public static class DTMPTextCreator
	{
		[MenuItem("GameObject/DUI/TMPText", false, 30)]
		public static void CreateDTMPText(MenuCommand menuCommand)
		{
			string path = "Assets/OuterRes/CustomUI/DTMPText.prefab";
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null) throw new Exception($"[CreateDTMPText]. no prefab in this path: {path}");

			GameObject tmpTextObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
			GameObjectUtility.SetParentAndAlign(tmpTextObj, menuCommand.context as GameObject);

			if (tmpTextObj.transform.parent == null)
			{
				Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
				if (canvas != null) tmpTextObj.transform.SetParent(canvas.transform, false);
				else throw new Exception("[CreateDTMPText]. please create canvas firstly");
			}

			PrefabUtility.UnpackPrefabInstance(tmpTextObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);

			Undo.RegisterCreatedObjectUndo(tmpTextObj, "Create DTMPText");
			Selection.activeGameObject = tmpTextObj;
		}
	}
}