namespace DingFrame.Module.AssetLoader
{
	using System.Threading.Tasks;
	using UnityEngine;

	public interface IAssetLoader
	{
		T Load<T>(string assetPath) where T : Object;
		Task<T> LoadAsync<T>(string assetPath) where T : Object;

		void Unload<T>(T asset) where T : Object;
	}
}
