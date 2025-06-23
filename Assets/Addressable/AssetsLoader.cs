using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RuGameFramework.Assets
{
	public class AssetsLoader : IAssetsLoader
	{
		private string _address;
		private AsyncOperationHandle _handle;
		// 加载过后的缓存
		private bool _isLoad = false;

		public AssetsLoader (string address)
		{
			if (address == null)
			{
				return;
			}

			_address = address;
			_isLoad = false;
		}

		public IEnumerator LoadPrefabAssetAsync(string path, Action<GameObject> onComplete, Action onFail = null)
		{
			if (_isLoad)
			{
				yield break;
			}

			_isLoad = true;
			_handle = Addressables.LoadAssetAsync<GameObject>(_address);
			yield return _handle;
			
			if (_handle.Status == AsyncOperationStatus.Succeeded)
			{
				onComplete?.Invoke(_handle.Result as GameObject);
			}
			else if (_handle.Status == AsyncOperationStatus.Failed)
			{
				onFail?.Invoke();
			}
		}

		public void LoadAsset<T> (Action<T> onComplete, Action onFail = null) where T : UnityEngine.Object
		{
			if (_isLoad)
			{
				if (_handle.IsDone)
				{
					onComplete?.Invoke(_handle.Result as T);
				}
				else
				{
					_handle.Completed += (result) =>
					{
						if (result.Status == AsyncOperationStatus.Succeeded)
						{
							onComplete?.Invoke(result.Result as T);
						} else if (result.Status == AsyncOperationStatus.Failed)
						{
							onFail?.Invoke();
						}
					};
				}
				return;
			}
			_isLoad = true;
			_handle = Addressables.LoadAssetAsync<T>(_address);
			_handle.Completed += (result) =>
			{
				if (result.Status == AsyncOperationStatus.Succeeded)
				{
					onComplete?.Invoke(result.Result as T);
				}
				else if (result.Status == AsyncOperationStatus.Failed)
				{
					onFail?.Invoke();
				}
			};
		}

		[Obsolete]
		public T LoadAsset<T> () where T : UnityEngine.Object
		{
			if (_isLoad)
			{
				return  _handle.WaitForCompletion() as T;
			}

			_isLoad = true;
			_handle = Addressables.LoadAsset<T>(_address);
			var ret =  _handle.WaitForCompletion() as T;
			return ret;
		}

		public void Release ()
		{
			if (!_isLoad || !_handle.IsValid())
			{
				return;
			}
			_isLoad = false;
			Addressables.Release(_handle);
		}


	}
}

