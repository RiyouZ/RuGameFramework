using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RuGameFramework.Assets
{
	public class AssetsCollator
	{
		private Dictionary<string, AsyncOperationHandle> _assetHandleDic;

		public AssetsCollator()
		{
			_assetHandleDic = new Dictionary<string, AsyncOperationHandle>();
		}

		// 添加外部读取的handle
		public void AddHandle(string path, AsyncOperationHandle handle)
		{
			_assetHandleDic.Add(path, handle);
		}
		
		// 代理读取资源
		public void LoadAsset<T> (string path, Action<T> onComplate, Action onFail = null) where T : UnityEngine.Object
		{
			var handle = AssetsManager.LoadAsset<T>(path, onComplate, onFail);
			_assetHandleDic.Add(path, handle);
		}

		public T GetAsset<T>(string path) where T : UnityEngine.Object
		{
			if (!_assetHandleDic.ContainsKey(path) || !_assetHandleDic[path].IsDone)
			{
				return null;
			}
			
			return _assetHandleDic[path].Result as T;
		}

		// 释放全部资源
		public void ReleaseAssets()
		{
			foreach (var handle in _assetHandleDic.Values)
			{
				if (handle.IsDone)
				{
					AssetsManager.Release(handle);
				}
			}
			_assetHandleDic.Clear();
		}
		
		// 释放指定资源
		public void ReleaseAsset(string path)
		{
			if (!_assetHandleDic.ContainsKey(path))
			{
				return;
			}
			AssetsManager.Release(_assetHandleDic[path]);
			_assetHandleDic.Remove(path);
		}

	}

}
