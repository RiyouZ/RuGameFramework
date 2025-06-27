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

		// ����Prefab�Ļ���  Ϊ���ͷ�handle Addressable�Ѿ�����Bundle�Ļ������ü��� ����Result�ķ�ʽ�ƻ���ԭ�е����ü���
		// private static Dictionary<string, IAssetsLoader> _loaderCache = new Dictionary<string, IAssetsLoader>(DefalutCapacity);
		
		private static Dictionary<string, AsyncOperationHandle> _handleCache = new Dictionary<string, AsyncOperationHandle>(DefalutCapacity);
		
		// ʵ�����ü���
		private static Dictionary<string, int> _gameObjectCount = new Dictionary<string, int>(DefalutCapacity);
		
		// ����prefab�Ļ���
		private static Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>(DefalutCapacity);
		
		// ʵ�������� �� prefab·��������
		private static Dictionary<int, string> _objDic = new Dictionary<int, string>(DefalutCapacity);
		
		// ʵ������ ��ֹ�����첽ʱһ֡������ͬ���ʵ����
		private static Dictionary<string, bool> _instantiateLock = new Dictionary<string, bool>(DefalutCapacity);
		
		// �ͷ��б�
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
			
			// ����ͷ���Դ
			if (_gameObjectCount[prefabName] <= 0)
			{
				_releaseList.Add(prefabName);
			}
		}

		// �첽ʵ����
		public static IEnumerator AsyncInstantiate(string prefabName, Action<GameObject> onGameObjectLoad, Transform parent = null)
		{
			Debug.LogWarning($"[AssetsManager] Loading prefab '{prefabName}' ");
			// �������ü���
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
			//  ���ڻ���ֱ�Ӽ���
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
				// ӳ��ʵ�������Ķ���ID
				_objDic.Add(obj.GetInstanceID(), prefabName);
				onGameObjectLoad(obj);
			});
			
			_instantiateLock[prefabName] = false;
		}
		
		// ͬ��״̬����Ԥ���ع�Prefab
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

		// ����
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
			
			// �ͷ�ʵ��������
			GameObject.Destroy(gameObject);
			_objDic.Remove(instanceId);
			RemoveObjectCount(prefabPath);
		}
		
		// ����Prefab��Դ Prefabδ���ع�ʱ����
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
		
		// �ͷ�
		public static void Release (AsyncOperationHandle handle)
		{
			Addressables.Release(handle);
			
			// ����ж��ʱ���Զ����� UnloadUnused
		}

		public static void Release (UnityEngine.Object asset)
		{
			Addressables.Release(asset);

			// ����ж��ʱ���Զ����� UnloadUnused
		}

		// �ͷ�δʹ�õ���Դ
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
