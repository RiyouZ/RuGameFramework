using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RuGameFramework.Assets
{
	public static class AssetsManager
	{
		private static int DefalutCapacity = 20;

		// 加载Prefab的缓存  为了释放handle Addressable已经做了Bundle的缓存引用计数 缓存Result的方式破坏了原有的引用计数
		// private static Dictionary<string, IAssetsLoader> _loaderCache = new Dictionary<string, IAssetsLoader>(DefalutCapacity);
		
		private static Dictionary<string, AsyncOperationHandle> _handleCache = new Dictionary<string, AsyncOperationHandle>(DefalutCapacity);
		
		// 实例引用计数
		private static Dictionary<string, int> _gameObjectCount = new Dictionary<string, int>(DefalutCapacity);
		
		// 加载prefab的缓存
		private static Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>(DefalutCapacity);
		
		// 实例化对象 对 prefab路径的索引
		private static Dictionary<int, string> _objDic = new Dictionary<int, string>(DefalutCapacity);
		
		// 实例化锁 防止出现异步时一帧申请相同多个实例化
		private static Dictionary<string, bool> _instantiateLock = new Dictionary<string, bool>(DefalutCapacity);
		
		// 释放列表
		private static List<string> _releaseList = new List<string>();
		
		private static void AddObjectCount(string prefabName)
		{
			if (!_gameObjectCount.ContainsKey(prefabName))
			{
				_gameObjectCount.Add(prefabName, 1);
				return;
			}
			
			_gameObjectCount[prefabName]++;
		}

		private static void RemoveObjectCount(string prefabName)
		{
			if (!_gameObjectCount.ContainsKey(prefabName))
			{
				return;
			}

			_gameObjectCount[prefabName]--;
			
			// 标记释放资源
			if (_gameObjectCount[prefabName] <= 0)
			{
				_releaseList.Add(prefabName);
			}
		}

		// 异步实例化
		public static IEnumerator AsyncInstantiate(string prefabName, Action<GameObject> onGameObjectLoad, Transform parent = null)
		{
			Debug.LogWarning($"[AssetsManager] Loading prefab '{prefabName}' ");
			// 增加引用计数
			AddObjectCount(prefabName);
			while (_instantiateLock.ContainsKey(prefabName) && _instantiateLock[prefabName])
			{
				yield return null;
			}

			if (!_instantiateLock.ContainsKey(prefabName))
			{
				_instantiateLock.Add(prefabName, false);
			}
			
			_instantiateLock[prefabName] = true;
			
			GameObject obj = null;
			//  存在缓存直接加载
			if (_prefabCache.TryGetValue(prefabName, out GameObject prefab))
			{
				var asyncHandle = GameObject.InstantiateAsync(prefab, parent);
				yield return asyncHandle;
				obj = asyncHandle.Result[0];
				_objDic.Add(obj.GetInstanceID(), prefabName);
				onGameObjectLoad(obj);
				yield break;
			}

			yield return AsyncLoadPrefabAsset(prefabName, (prefabObj) =>
			{
				obj = GameObject.Instantiate(prefabObj, parent);
				// 映射实例化过的对象ID
				_objDic.Add(obj.GetInstanceID(), prefabName);
				onGameObjectLoad(obj);
			});
			
			_instantiateLock[prefabName] = false;
		}
		
		// 同步状态必须预加载过Prefab
		public static GameObject Instantiate(string prefabName)
		{
			AddObjectCount(prefabName);
			if (_prefabCache.TryGetValue(prefabName, out GameObject prefab))
			{
				return GameObject.Instantiate(prefab);
			}
			
# if UNITY_EDITOR
				Debug.LogErrorFormat("[AssetsManager] UnLoading prefab '{0}' failed.", prefabName);
#endif
			
			return null;
		}

		// 销毁
		public static void Destroy (GameObject gameObject)
		{
			int instanceId = gameObject.GetInstanceID();
			if (!_objDic.TryGetValue(instanceId, out string prefabPath))
			{
#if UNITY_EDITOR
				Debug.LogErrorFormat("[AssetsManager] Destory prefab '{0}' failed.", gameObject.name);
#endif
				return;
			}
			
			// 释放实例的索引
			GameObject.Destroy(gameObject);
			_objDic.Remove(instanceId);
			RemoveObjectCount(prefabPath);
		}
		
		// 加载Prefab资源 Prefab未加载过时调用
		private static IEnumerator AsyncLoadPrefabAsset (string path, Action<GameObject> onComplate = null, Action onFail = null)
		{
			if (_handleCache.ContainsKey(path))
			{
				yield break;
			}
			
			var opHandle = Addressables.LoadAssetAsync<GameObject>(path); 
			_handleCache.Add(path, opHandle);
			yield return opHandle;
			opHandle.Completed += (result) =>
			{
				if (result.Status == AsyncOperationStatus.Succeeded)
				{
					_prefabCache.Add(path, result.Result);
					onComplate?.Invoke(result.Result);
				}else if (result.Status == AsyncOperationStatus.Failed)
				{
					onFail?.Invoke();
					Addressables.Release(opHandle);
				}
			};
		}

		public static AsyncOperationHandle LoadAsset<T> (string path, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			AsyncOperationHandle<T> opHandle = Addressables.LoadAssetAsync<T>(path);
			opHandle.Completed += (result) =>
			{
				if (result.Status == AsyncOperationStatus.Succeeded)
				{
					onComplate?.Invoke(result.Result);
				}else if (result.Status == AsyncOperationStatus.Failed)
				{
					onFail?.Invoke();
					Addressables.Release(opHandle);
				}
			};
			return opHandle;
		}

		public static AsyncOperationHandle LoadAsset<T> (AssetReference assetRef, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			string path = assetRef.ToString();

			var opHandle = assetRef.LoadAssetAsync<T>();
			
			opHandle.Completed += (result) =>
			{
				if (result.Status == AsyncOperationStatus.Succeeded)
				{
					onComplate?.Invoke(result.Result);
				}else if (result.Status == AsyncOperationStatus.Failed)
				{
					onFail?.Invoke();
					assetRef.ReleaseAsset();
				}
			};

			return opHandle;
		}

		public static void Release (AssetReference assetRef)
		{
			assetRef.ReleaseAsset();
		}
		
		// 释放
		public static void Release (AsyncOperationHandle handle)
		{
			Addressables.Release(handle);
			
			// 场景卸载时会自动调用 UnloadUnused
		}

		public static void Release (UnityEngine.Object asset)
		{
			Addressables.Release(asset);

			// 场景卸载时会自动调用 UnloadUnused
		}

		// 释放未使用的资源
		public static void ReleaseUnusedPrefabs ()
		{
			foreach(var prefabPath in _releaseList)
			{
				Release(_handleCache[prefabPath]);
				
				_handleCache.Remove(prefabPath);
				_gameObjectCount.Remove(prefabPath);
				_prefabCache.Remove(prefabPath);
				_instantiateLock.Remove(prefabPath);
			}
		}

	}

}
