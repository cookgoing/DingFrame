namespace DingFrame.Module.Ani
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	public class AnimatorEventBehaviour : StateMachineBehaviour
	{
		private static readonly Dictionary<object, Action<object, Animator, AnimatorStateInfo, int>> actionDic = new();
		private static readonly Dictionary<object, Action<object, Animator, AnimatorStateInfo, int>> _cache = new ();

		public static void AddAction(object obj, Action<object, Animator, AnimatorStateInfo, int> action) => actionDic[obj] = action;
		public static void RemoveAction(object obj, Action<object, Animator, AnimatorStateInfo, int> action) => actionDic.Remove(obj);

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateUpdate(animator, stateInfo, layerIndex);

			_cache.Clear();
			foreach (var kv in actionDic) _cache.Add(kv.Key, kv.Value);
			foreach (var kv in _cache) kv.Value(kv.Key, animator, stateInfo, layerIndex);
		}
	}
}