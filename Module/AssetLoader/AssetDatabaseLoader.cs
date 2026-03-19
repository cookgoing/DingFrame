#if UNITY_EDITOR
namespace DingFrame.Module.AssetLoader
{
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEditor;

	public sealed class AssetDatabaseLoader : IAssetLoader
	{
		/*
		All paths are relative to the project folder, for example: "Assets/MyTextures/hello.png".
		Note:
			The assetPath parameter is not case sensitive.
			ALL asset names and paths in Unity use forward slashes, even on Windows.
			This returns only an asset object that is visible in the Project view. If the asset is not found LoadAssetAtPath returns Null.
		*/
		public T Load<T>(string assetPath) where T : Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}

		public async Task<T> LoadAsync<T>(string assetPath) where T : Object
		{
			await Task.CompletedTask;
			return AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}

		public void Unload<T>(T asset) where T : Object {}
	}
}
#endif