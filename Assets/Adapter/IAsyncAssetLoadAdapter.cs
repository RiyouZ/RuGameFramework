using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Assets
{
    public interface IAsyncAssetLoadAdapter 
    {
        public Coroutine AsyncLoadAsset<T>(string assetName, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object;
        public Coroutine AsyncLoadPrefab(string assetName, Action<GameObject> onComplate, Transform parent = null);
        public void Destroy(GameObject prefab);

        // 因异步需启动协程 通过返回的协程关闭
        public void StopCoroutine(Coroutine coroutine);
    }
}
