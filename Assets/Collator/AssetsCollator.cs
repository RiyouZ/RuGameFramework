using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RuGameFramework.Assets
{
	public class AAssetsProxy : IAssetsManager
	{		
		// 代理读取资源
		public void LoadAsset<T> (string path, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			AssetsManager.LoadAsset<T>(path, onComplate, onFail);
		}

		public void Destroy (GameObject prefab)
		{
			AssetsManager.Destroy(prefab);
		}

		public GameObject Instantiate (GameObject prefab)
		{
			return AssetsManager.Instantiate(prefab.name);
		}

		public GameObject Instantiate (string prefab)
		{
			return AssetsManager.Instantiate(prefab);
		}

		public IEnumerator AsyncInstantiate (string prefab, Action<GameObject> onComplate, Transform parent = null)
		{
			yield return AssetsManager.AsyncInstantiate(prefab, onComplate, parent);
		}
	}

}
