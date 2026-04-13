namespace DingFrame.Module.AssetLoader
{
	using System.Threading.Tasks;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using DingFrame;

	public static class AddressablesManager
	{
		private static Dictionary<string, AsyncOperationHandle> handles = new();

		public static async Task<T> LoadAsync<T>(string assetPath) where T : Object
		{
			if (handles.TryGetValue(assetPath, out var existingHandle)) return existingHandle.Result as T;

			var handle = Addressables.LoadAssetAsync<T>(assetPath);
			await handle.Task;

			if (handle.Status != AsyncOperationStatus.Succeeded)
			{
				DLogger.Error($"[LoadAsync]. Load failed: {assetPath}; Status: {handle.Status}");
				Addressables.Release(handle);
				return default;
			}

			handles[assetPath] = handle;
			return handle.Result;
		}

		public static bool Unload(string assetPath)
		{
			if (!handles.TryGetValue(assetPath, out var handle)) return false;

			Addressables.Release(handle);
			handles.Remove(assetPath);
			return true;
		}
	}
}