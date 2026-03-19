namespace DingFrame.Module.Audio
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Audio;
	using DingFrame.Order;
	using DingFrame.Module.Coroutine;

	public struct AudioInfo
	{
		public int Type;
		public string Path;
		public Transform Parent;
		public AudioClip Clip;
		public AudioMixerGroup MixerGroup;
		public bool Loop;
		public int Priority;
		public float Volume;
		public float Pitch;
		public float SpatialBlend;
		public Action OnComplete;
	}

	public sealed class AudioModule : MonoBehaviour, IModule
	{
		public const int MAX_SOURCES_COUNT = 32;

		public static AudioModule Create()
		{
			GameObject gameLaunch = GameObject.FindWithTag(FrameConfigure.GAME_LAUNCH_TAG);
			if (gameLaunch == null)
			{
				DLogger.Error("gameManager is not found", "SoundModule");
				return null;
			}

			GameObject soundObj = new(FrameConfigure.AUDIO_MODULE_NAME);
			soundObj.tag = FrameConfigure.AUDIO_TAG;
			soundObj.transform.parent = gameLaunch.transform;

			return soundObj.AddComponent<AudioModule>();
		}

		public static void AudioAssign(AudioSource source, AudioInfo info)
		{
			source.clip = info.Clip;
			source.outputAudioMixerGroup = info.MixerGroup;
			source.loop = info.Loop;
			source.priority = info.Priority;
			source.volume = info.Volume;
			source.pitch = info.Pitch;
			source.spatialBlend = info.SpatialBlend;
		}

		private Order listenOrder = Order.CreateOrder(FrameConfigure.GAMESTATE_DEFAULT_ORDER);
		public Order ListenOrder => listenOrder;

		private AudioListener audioListener;
		private Queue<AudioSource> audioSources;
		private Dictionary<AudioSource, (AudioInfo, IEnumerator)> playingAudio;
		private Dictionary<AudioSource, AudioInfo> pauseAudio;
		private int sourceCountInUse;
		private CoroutineModule _coroutineModule;
		private CoroutineModule coroutineModule => _coroutineModule ??= ModuleCollector.GetModule<CoroutineModule>();

		public void Init()
		{
			audioListener = gameObject.AddComponent<AudioListener>();
			audioSources = new(MAX_SOURCES_COUNT);
			playingAudio = new(MAX_SOURCES_COUNT);
			pauseAudio = new();
			sourceCountInUse = 0;
		}
		public void Dispose()
		{
			Destroy(audioListener);
			foreach (var kv in playingAudio)
			{
				coroutineModule.ExitCoroutine(kv.Value.Item2);
				ReturnAudioSource(kv.Key);
			}
			foreach (var kv in pauseAudio) ReturnAudioSource(kv.Key);
			while (audioSources.TryDequeue(out AudioSource source)) Destroy(source);

			sourceCountInUse = 0;
			playingAudio.Clear();
			pauseAudio.Clear();
			audioSources.Clear();
		}

		private AudioSource GetAudioSource(Transform parent)
		{
			if (sourceCountInUse >= MAX_SOURCES_COUNT) DLogger.Error($"audio source exceed max count: {MAX_SOURCES_COUNT}");

			if (!audioSources.TryDequeue(out AudioSource source))
			{
				GameObject soundObj = new($"Audio_{sourceCountInUse}");
				source = soundObj.AddComponent<AudioSource>();
			}

			sourceCountInUse++;
			source.transform.SetParent(parent ?? transform, false);
			source.enabled = true;
			return source;
		}
		private bool ReturnAudioSource(AudioSource source)
		{
			if (source == null) return false;
			if (audioSources.Contains(source)) return false;

			source.Stop();
			source.clip = null;
			source.enabled = false;
			audioSources.Enqueue(source);
			sourceCountInUse--;
			return true;
		}
		private IEnumerator ReturnAudioSource(AudioSource source, float delaySec, Action onComplete)
		{
			yield return new WaitForSeconds(delaySec);

			onComplete?.Invoke();
			ReturnAudioSource(source);
		}

		public AudioSource PlayAudio(AudioInfo info)
		{
			if (info.Clip == null) return null;

			AudioSource source = GetAudioSource(info.Parent);
			AudioAssign(source, info);

			if (!info.Loop)
			{
				IEnumerator ie = ReturnAudioSource(source, info.Clip.length, info.OnComplete);
				playingAudio[source] = (info, ie);
				coroutineModule.RunCoroutine(ie);
			}
			source.Play();
			return source;
		}
		public void StopAudio(AudioSource source)
		{
			if (!playingAudio.TryGetValue(source, out var info))
			{
				DLogger.Error($"the source is not in playing. can not stop", "AudioModule.StopAudio");
				return;
			}

			coroutineModule.ExitCoroutine(info.Item2);
			playingAudio.Remove(source);
			ReturnAudioSource(source);
		}
		public void PauseResumeAudio(AudioSource source, bool isPause)
		{
			if (isPause)
			{
				if (!playingAudio.TryGetValue(source, out var info))
				{
					DLogger.Error($"the source is not in playing. can not pause", "AudioModule.PauseResumeAudio");
					return;
				}

				source.Pause();
				playingAudio.Remove(source);
				coroutineModule.ExitCoroutine(info.Item2);
				pauseAudio[source] = info.Item1;
			}
			else
			{
				if (!pauseAudio.TryGetValue(source, out AudioInfo info))
				{
					DLogger.Error($"the source is not in pause. can not resume", "AudioModule.PauseResumeAudio");
					return;
				}

				source.UnPause();
				pauseAudio.Remove(source);
				IEnumerator ie = ReturnAudioSource(source, info.Clip.length, info.OnComplete);
				playingAudio[source] = (info, ie);
				coroutineModule.RunCoroutine(ie);
			}
		}
	
		public Dictionary<AudioSource, (AudioInfo, IEnumerator)> FilterPlayingAudio(Predicate<AudioInfo> match)
		{
			Dictionary<AudioSource, (AudioInfo, IEnumerator)> result = new();
			foreach (var kv in playingAudio)
				if (match(kv.Value.Item1)) 
					result[kv.Key] = kv.Value;
			
			return result;
		}
	}
}