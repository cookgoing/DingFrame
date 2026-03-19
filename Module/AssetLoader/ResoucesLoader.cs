namespace DingFrame.Module.AssetLoader
{
	using System.Collections;
	using System.Threading.Tasks;
	using UnityEngine;
	using DingFrame.Module.Coroutine;

	public sealed class ResoucesLoader : IAssetLoader
	{
		/*
			The path is relative to any folder named Resources inside the Assets folder of your project.
			returns the asset at path if it can be found, otherwise it returns null.
			note: the path is case insensitive and must not contain a file extension. All asset names and paths in Unity use forward slashes
		*/
		public T Load<T>(string assetPath) where T : Object => Resources.Load<T>(assetPath);

		public async Task<T> LoadAsync<T>(string assetPath) where T : Object
		{
			ResourceRequest resRequest = Resources.LoadAsync<T>(assetPath);
			await resRequest;
			return resRequest?.asset as T ?? null;
		}

		public void Unload<T>(T asset) where T : Object => Resources.UnloadAsset(asset);
	}
}