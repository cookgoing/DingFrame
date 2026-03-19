namespace DingFrame.Utils
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.Networking;
	using UnityEngine.UIElements;

	public struct FrameEventArg { }

	public static class MonoUtils
	{
		public static bool RemoveAllComponents<T>(this GameObject gameObject) where T : Component
		{
			T[] components = gameObject.GetComponents<T>();
			if (components == null || components.Length == 0) return false;

			foreach (T com in components) UnityEngine.Object.Destroy(com);
			return true;
		}

		public static T AddComponentIfNo<T>(this GameObject gameObject) where T : Component => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

		public static Color Str2Color(string colorStr)
		{
			if (!ColorUtility.TryParseHtmlString(colorStr, out Color color)) throw new Exception($"[error][Str2Color]. {colorStr} can not parse to Color, will use default color");

			return color;
		}

		public static string Color2Str(Color color) => ColorUtility.ToHtmlStringRGB(color);


		public static EventCallback<TEventType> WrapEventCallback<TEventType>(EventCallback<TEventType, FrameEventArg> evt, bool stopPropagation) where TEventType : EventBase<TEventType>, new()
		{
			return et =>
			{
				if (stopPropagation) et.StopPropagation();

				evt(et, new FrameEventArg());
			};
		}


		public static void ForceTKScrollViewUpdateSize(ScrollView view)
		{
			view.schedule.Execute(() =>
			{
				var fakeOldRect = Rect.zero;
				var fakeNewRect = view.layout;

				using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
				evt.target = view.contentContainer;
				view.contentContainer.SendEvent(evt);
			});
		}

		public static ScrollView GetScrollViewByContent(VisualElement contentContainer)
		{
			VisualElement parent = contentContainer;
			while (parent != null)
			{
				if (parent is ScrollView scrollView) return scrollView;

				parent = parent.parent;
			}

			return null;
		}

		public static (float widthRatio, float heightRatio) FullTexture(Vector2 uiSize, Vector2 textureSize)
		{
			if (uiSize.x == 0 || textureSize.x == 0) return (1, 1);

			float uiHeightRatio = uiSize.y / uiSize.x;
			float textureHeightRatio = textureSize.y / textureSize.x;

			if (uiHeightRatio <= textureHeightRatio) return (1, textureHeightRatio / uiHeightRatio);
			else return (uiHeightRatio / textureHeightRatio, 1);
		}

		public static Texture2D MakeGrayscaleCopy(Texture2D src, bool isGamma = false)
		{
			if (!src || !src.isReadable) return null;

			var fmt = TextureFormat.RGBA32;
			var tmp = new Texture2D(src.width, src.height, fmt, false);
			tmp.name = src.name + "(GaryClone)";
			var px = src.GetPixels32();
			for (int i = 0; i < px.Length; i++)
			{
				var c = px[i];
				byte g = isGamma ? (byte)Mathf.Clamp(Mathf.RoundToInt(0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b), 0, 255)
								: (byte)Mathf.Clamp(Mathf.RoundToInt(0.299f * c.r + 0.587f * c.g + 0.114f * c.b), 0, 255);
				px[i] = new Color32(g, g, g, c.a);
			}
			tmp.SetPixels32(px);
			tmp.Apply(false, false);
			return tmp;
		}

		public static Vector2 ParseVector2(string str)
		{
			str = str.TrimStart('(').TrimEnd(')');
			Vector2 result = Vector2.zero;
			string[] parts = str.Split(',');
			if (parts.Length >= 2
			&& float.TryParse(parts[0], out float x)
			&& float.TryParse(parts[1], out float y)) result = new Vector2(x, y);
			else Debug.LogError($"[ParseVector2]. str: {str} can not parse to Vector2, will use default Vector2.zero");

			return result;
		}
		public static Vector3 ParseVector3(string str)
		{
			str = str.TrimStart('(').TrimEnd(')');
			Vector3 result = Vector3.zero;
			string[] parts = str.Split(',');
			if (parts.Length >= 3
			&& float.TryParse(parts[0], out float x)
			&& float.TryParse(parts[1], out float y)
			&& float.TryParse(parts[2], out float z)) result = new Vector3(x, y, z);
			else Debug.LogError($"[ParseVector3]. str: {str} can not parse to Vector3, will use default Vector3.zero");

			return result;
		}

		public static bool IsReallyVisible(VisualElement ele)
		{
			if (ele == null) return false;
			if (!ele.visible) return false;
			if (ele.style.display == DisplayStyle.None) return false;

			VisualElement parent = ele.parent;
			while (parent != null)
			{
				if (!parent.visible) return false;
				if (parent.style.display == DisplayStyle.None) return false;

				parent = parent.parent;
			}

			return true;
		}

		public static float SoundLinear2DB(float volume)
		{
			float v = Mathf.Clamp(volume, 0.0001f, 1f);
			float dB = Mathf.Log10(v) * 20f;
			return dB;
		}
	
		public static async Task<(bool isError, Texture2D texture)> DownloadTexture(string url, CancellationToken? cancellationToken)
		{
			try
			{
				UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
				var operation = uwr.SendWebRequest();

				while (!operation.isDone) await Task.Yield();
				cancellationToken?.ThrowIfCancellationRequested();

				if (uwr.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Failed to load texture: {uwr.error}; url: {url}");
					return (true, null);
				}

				Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
				uwr.Dispose();
				return (false, texture);
			}
			catch (OperationCanceledException)
			{
				return (true, null);
			}
			catch(Exception e)
			{
				Debug.LogError($"[DownloadTexture] has error. {MiscUtils.ExceptionStr(e)}");
				return (true, null);
			}
		}
	}
}