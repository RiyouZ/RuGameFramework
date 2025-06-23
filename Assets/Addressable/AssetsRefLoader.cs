using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RuGameFramework.Assets
{
	public class AssetsRefLoader : IAssetsLoader
	{
		private string _address;
		private AsyncOperationHandle _handle;
		private bool _isLoad;
		private AssetReference _assetRef;

		public AssetsRefLoader (AssetReference assetRef)
		{
			if (assetRef == null)
			{
				return;
			}

			_assetRef = assetRef;
			_address = _assetRef.ToString();
			_isLoad = false;
		}

		public IEnumerator LoadPrefabAssetAsync(string path, Action<GameObject> onComplete, Action onFail = null)
		{	
			if (_isLoad)
			{
				yield break;
			}

			_isLoad = true;
			_handle = _assetRef.LoadAssetAsync<GameObject>();
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
						}
						else if (result.Status == AsyncOperationStatus.Failed)
						{
							onFail?.Invoke();
						}
					};
				}
				return;
			}
			_isLoad = true;
			_handle = _assetRef.LoadAssetAsync<T>();
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

		public void Release ()
		{
			if (!_isLoad || !_handle.IsValid())
			{
				return;
			}
			_isLoad = false;
			_assetRef.ReleaseAsset();
		}
	}
}

