namespace DingFrame.Module.AssetLoader
{
	using System;
	using System.Collections;
	using System.Threading.Tasks;
	using UnityEngine;
	using DingFrame.Module.Coroutine;

	// all asset path is same as AssetDatabaase. such as: "Assets/MyTextures/hello.png".
	public sealed class AssetLoadModule : IModule
	{
		private IAssetLoader assetLoader;

		public void RegisterResourceLoader(IAssetLoader assetLoader) => this.assetLoader = assetLoader;

		public T Load<T>(string assetPath) where T : UnityEngine.Object
		{
			if (assetLoader == null)
			{
				DLogger.Error("no resourceLoader", "ResourceLoadModule");
				return null;
			}

			return assetLoader.Load<T>(AssetPathParse(assetPath));
		}

		public Task<T> LoadAsync<T>(string assetPath) where T : UnityEngine.Object
		{
			if (assetLoader == null)
			{
				DLogger.Error("no resourceLoader", "ResourceLoadModule");
				return null;
			}

			return assetLoader.LoadAsync<T>(AssetPathParse(assetPath));
		}

		public void Unload<T>(T asset) where T : UnityEngine.Object
		{
			if (assetLoader == null)
			{
				DLogger.Error("no resourceLoader", "ResourceLoadModule");
				return;
			}

			assetLoader.Unload<T>(asset);
		}

		public async Task UnloadUnusedAsset() => await Resources.UnloadUnusedAssets();


		private string AssetPathParse(string assetPath)
		{
			if (assetLoader is ResoucesLoader)
			{
				ReadOnlySpan<char> span = assetPath.AsSpan();
				string resPattern = "Resources/";
				int resIdx = span.IndexOf(resPattern);
				int suffixIdx = span.IndexOf('.');
				int startIdx = resIdx + resPattern.Length;
				int length = span.Length - startIdx - (suffixIdx == -1 ? 0 : (span.Length - suffixIdx));

				if (resIdx != -1) span = span.Slice(startIdx, length);

				return span.ToString();
			}

			return assetPath;
		}
	}
}