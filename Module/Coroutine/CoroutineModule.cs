namespace DingFrame.Module.Coroutine
{
	using System.Collections;
	using System.Threading.Tasks;
	using UnityEngine;

	public sealed class CoroutineModule : MonoBehaviour, IModule
	{
		public static CoroutineModule Create()
		{
			GameObject gameLaunch = GameObject.FindWithTag(FrameConfigure.GAME_LAUNCH_TAG);
			if (gameLaunch == null)
			{
				DLogger.Error("gameManager is not found", "CoroutineModule");
				return null;
			}

			return gameLaunch.AddComponent<CoroutineModule>();
		}


		public async Task RunCoroutineAsTask(IEnumerator routine)
		{
			TaskCompletionSource<bool> tcs = new();
			StartCoroutine(RunCoroutine(routine, tcs));
			await tcs.Task;
		}

		public IEnumerator RunCoroutine(IEnumerator routine, TaskCompletionSource<bool> tcs)
		{
			yield return StartCoroutine(routine);
			tcs.SetResult(true);
		}


		public void RunCoroutine(IEnumerator routine) => StartCoroutine(routine);
		public void ExitCoroutine(IEnumerator routine) => StopCoroutine(routine);
		public void ExitAllCoroutine() => StopAllCoroutines();
	}
}