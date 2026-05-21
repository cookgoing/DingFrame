namespace DingFrame
{
	using System;
	using System.IO;
	using System.Linq;
	using UnityEditor;
	using UnityEditor.AddressableAssets;
	using UnityEditor.AddressableAssets.Settings;
	using UnityEngine;

	public static class GenerateAddressablesGroup
	{
		private const string BasePath = "Assets/AARes";
		private readonly static string[] InnerGroup = new string[] {"unifiedraytracing"};
		private readonly static string[] DefaultGroup = new string[]{"local"};
		
		[MenuItem("Tools/Addressables/Clear Groups")]
		public static void ClearAAResGroups()
		{
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
			{
				Debug.LogError("没有找到 AddressableAssetSettings，请先打开 Addressables Window 创建设置。");
				return;
			}

			var groupsToRemove = settings.groups.Where(g => Array.Find(InnerGroup, name => name == g.name) == null).ToList();

			foreach (var group in groupsToRemove)
			{
				foreach (var entry in group.entries.ToList()) settings.RemoveAssetEntry(entry.guid);

				settings.RemoveGroup(group);
			}

			settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
			AssetDatabase.SaveAssets();
		}

		[MenuItem("Tools/Addressables/Generate Groups")]
		public static void GenerateGroups()
		{
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
			if (settings == null)
			{
				Debug.LogError("没有找到 AddressableAssetSettings，请先打开 Addressables Window 创建设置。");
				return;
			}

			string[] firstLevelDirs = Directory.GetDirectories(BasePath);
			foreach (string dirPath in firstLevelDirs)
			{
				string groupName = Path.GetFileName(dirPath);
				Debug.Log($"处理目录: {dirPath} -> Group: {groupName}");

				AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.Name == groupName);
				if (group == null)
				{
					bool isDefault = Array.Find(DefaultGroup, name => name == groupName) != null;
					bool readOnly = !isDefault;
					group = settings.CreateGroup(groupName, isDefault, readOnly, false, null, typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema));
					Debug.Log($"创建 Group: {groupName}");
				}

				string[] assetPaths = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories).Where(f => !f.EndsWith(".meta")).ToArray();
				foreach (string assetPath in assetPaths)
				{
					string guid = AssetDatabase.AssetPathToGUID(assetPath);
					if (string.IsNullOrEmpty(guid)) continue;

					AddressableAssetEntry entry = settings.FindAssetEntry(guid);
					if (entry == null)
					{
						entry = settings.CreateOrMoveEntry(guid, group);
						Debug.Log($"添加资源: {assetPath} 到 Group: {groupName}");
					}
					else
					{
						entry.parentGroup = group;
					}

					// 设置 Label
					// if (!entry.labels.Contains(groupName))
					// {
					// 	entry.SetLabel(groupName, true, true);
					// }
				}
			}

			settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
			AssetDatabase.SaveAssets();
			Debug.Log("Addressables 分组完成！");
		}
	}

}