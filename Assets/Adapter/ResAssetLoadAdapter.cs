using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Assets
{
	public class ResAssetLoadAdapter : IAsyncAssetLoadAdapter
	{
		private ResManager _resManager;
		private MonoBehaviour _runner;

		public ResAssetLoadAdapter(MonoBehaviour runner)
		{
			_resManager = ResManager.Instance;
			_resManager.Runner = runner;
			_runner = runner;
		}

		public Coroutine AsyncLoadAsset<T> (string assetName, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			return _runner.StartCoroutine(_resManager.LoadAssets<T>(assetName, onComplate));
		}

		public Coroutine AsyncLoadPrefab (string assetName, Action<GameObject> onComplate, Transform parent = null)
		{
			return _runner.StartCoroutine(_resManager.AsyncInstantiate(assetName, onComplate));
		}

		public void Destroy (GameObject prefab)
		{
			_resManager.Destroy(prefab);
		}

		public void StopCoroutine (Coroutine coroutine)
		{
			_runner.StopCoroutine(coroutine);
		}
	}

}
