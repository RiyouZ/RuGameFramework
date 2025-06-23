using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Assets
{
	public interface IAssetsLoader
	{
		public IEnumerator LoadPrefabAssetAsync(string path, Action<GameObject> onComplete, Action onFail = null);
		public void LoadAsset<T> (Action<T> onComplete, Action onFail = null) where T : UnityEngine.Object;
		public void Release ();

	}
}

