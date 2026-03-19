namespace DingFrame
{
	using UnityEngine;

	public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		protected static T _instance;
		public static T Instance { get => _instance ??= CreateInstance(); }

		public static T CreateInstance()
		{
			if (_instance != null) return _instance;

			GameObject obj = new GameObject(typeof(T).ToString());
			return obj.AddComponent<T>();
		}

		protected virtual void Awake()
		{
			_instance = this as T;
		}

		protected virtual void OnDestroy()
		{
			_instance = null;
		}
	}
}
