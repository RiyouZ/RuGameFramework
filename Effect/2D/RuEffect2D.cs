using RuGameFramework.Assets;
using RuGameFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Effect
{
	public static class RuEffect2D
	{
		private static Dictionary<string, IObjectPool<GameObject>> _effectCache = new Dictionary<string, IObjectPool<GameObject>>();

		private static Transform _effectPool;

		// ������Ч
		public static void LoadEffect (string path)
		{
			if (_effectCache.TryGetValue(path, out IObjectPool<GameObject> pool))
			{
				return;
			}

			if (_effectPool == null)
			{
				_effectPool = new GameObject("EffectPool").transform;
			}

			IObjectPool<GameObject> objPool = new GameObjectPool(path, _effectPool, 2, true);
			_effectCache.Add(path, objPool);
		}

		// ж����Ч
		public static void UnLoadEffect (string path)
		{
			if (!_effectCache.TryGetValue(path, out IObjectPool<GameObject> pool))
			{
				return;
			}

			pool.Dispose();
			_effectCache.Remove(path);
		}

		// ������Ч
		public static IEffect CreateEffect (string path, Action<IEffect> onCreate = null, Action<IEffect> onDestory = null)
		{
			IEffect effectCom = null;
			if (!_effectCache.TryGetValue(path, out IObjectPool<GameObject> pool))
			{
#if UNITY_EDITOR
				Debug.LogError($"��Ч {path} δ����");
#endif
				return null;
			}
			effectCom = pool.Spawn().GetComponent<IEffect>();
			effectCom.PrefabPath = path;
			effectCom.GameObject.SetActive(false);
			if (onCreate != null)
			{
				effectCom.OnCreate(onCreate);
			}

			if (onDestory != null)
			{
				effectCom.OnDestory(onDestory);
			}

			return effectCom;
		}

		public static IEffect CreateEffect (string path, string aniName, Action<IEffect> onCreate = null, Action<IEffect> onDestory = null)
		{
			var effect = CreateEffect(path, onCreate, onDestory);
			if (effect == null)
			{
				return null;
			}
			effect.EffectName = aniName;
			return effect;
		}

		// ������Ч
		public static void DestoryEffect (IEffect effect)
		{
			if (!_effectCache.TryGetValue(effect.PrefabPath, out IObjectPool<GameObject> pool))
			{
				return;
			}

			pool.Collection(effect.GameObject);
		}

	}
}

