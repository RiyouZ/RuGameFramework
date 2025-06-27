using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace RuGameFramework.Scene
{
	public class SceneLoader
	{
		private string _address;

		private AsyncOperationHandle<SceneInstance> _handle;
		public AsyncOperationHandle<SceneInstance> Handle => _handle;

		// 加载过后的缓存
		private bool _isLoad = false;

		public SceneLoader (string address)
		{
			if (address == null)
			{
				return;
			}

			_address = address;
			_isLoad = false;
		}

		public void LoadScene (LoadSceneMode loadMode = LoadSceneMode.Additive)
		{
			if (_isLoad)
			{
				return;
			}
			_handle = Addressables.LoadSceneAsync(_address, loadMode, false);
			_isLoad = true;
		}
		
		public void UnLoadScene ()
		{
			if (!_isLoad)
			{
				return;
			}

			// 只卸载场景资源
			UnityEngine.Resources.UnloadUnusedAssets();
		}

		public void Release ()
		{
			if (!_isLoad || !_handle.IsValid())
			{
				return;
			}

			var handle = Addressables.UnloadSceneAsync(_handle);
			handle.Completed += (result) =>
			{
				UnityEngine.Resources.UnloadUnusedAssets();
			};
		}

	}

}

