using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace  RuGameFramework.Assets
{
	public class AAAssetLoadAdapter : IAsyncAssetLoadAdapter
	{
		private MonoBehaviour _runner;

		public AAAssetLoadAdapter(MonoBehaviour runner)
		{
			this._runner = runner;
		}

		public Coroutine AsyncLoadAsset<T>(string assetName, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			return _runner.StartCoroutine(AssetsManager.LoadAsset<T>(assetName, onComplate, onFail));
		}

		public Coroutine AsyncLoadPrefab(string assetName, Action<GameObject> onComplate, Transform parent = null)
		{
			return _runner.StartCoroutine(AssetsManager.AsyncInstantiate(assetName, onComplate, parent));
		}

		public void Destroy (GameObject prefab)
		{
			AssetsManager.Destroy(prefab);
		}

		public void StopCoroutine(Coroutine coroutine)
		{
			_runner.StopCoroutine(coroutine);
		}

	}
}


