using Game.GamePlay.Factory;
using RuGameFramework.Args;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace RuGameFramework.Scene
{
	public class AASceneManager : MonoBehaviour
	{
		private static int MaxCapacity = 2;

		// 默认初始场景
		private string _defaultScene;
		public string DefaultScene => _defaultScene;

		// 当前场景
		private string _curSceneName;
		public string CurrentSceneName => _curSceneName;

		// 缓存场景持有的系统 id - 场景系统
		private Dictionary<string, BaseScene> _sysDic = new Dictionary<string, BaseScene>(MaxCapacity);

		// 场景缓存
		private Dictionary<string, SceneLoader> _sceneLoaderCache = new Dictionary<string, SceneLoader>(MaxCapacity);

		// 场景LRU记录
		private Dictionary<string, LinkedListNode<string>> _lruDic = new Dictionary<string, LinkedListNode<string>>(MaxCapacity);
		private LinkedList<string> _lruCache = new LinkedList<string>();

		// 缓存数量
		private int _cacheCapacity = 1;

		// 场景进度
		private float _loadProgress;
		public float Progress => _loadProgress;

		// 场景进度更新协程
		private Coroutine _asyncUpdateProgressHandle = null;

		public void SetDefaultScene (string sceneName)
		{
			_defaultScene = sceneName;
			_curSceneName = _defaultScene;
		}

		public void LoadDefaultScene (Action onComplate = null)
		{
			if (_asyncUpdateProgressHandle != null)
			{
				return;
			}

			if (_curSceneName == _defaultScene)
			{
				return;
			}
			string unloadScene = _curSceneName;
			_curSceneName = _defaultScene;

			var handle = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_defaultScene, LoadSceneMode.Single);
			_asyncUpdateProgressHandle = StartCoroutine(UpdateProgress(_defaultScene, handle, () =>
			{
				ClearCache();
				onComplate?.Invoke();
			}));
		}

		public void UnLoadDefaultScene ()
		{
			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_defaultScene);
		}

		public BaseScene GetSceneSystem ()
		{
			BaseScene sceneSys = null;
			if (!_sysDic.TryGetValue(_curSceneName, out sceneSys))
			{
				return null;
			}
			return sceneSys;
		}

		public BaseScene GetSceneSystem (string sceneName)
		{
			BaseScene sceneSys = null;
			if (!_sysDic.TryGetValue(sceneName, out sceneSys))
			{
				return null;
			}
			return sceneSys;
		}

		public SceneLoader GetSceneLoader (string sceneName)
		{
			if (!_sceneLoaderCache.TryGetValue(sceneName, out SceneLoader loader))
			{
				return null;
			}
			return loader;
		}

		// 场景切换 加载完后卸载场景
		public void SwitchScene<T> (string sceneName, ISceneArgs args = null, Action onComplate = null) where T : BaseScene, new ()
		{
			var unLoadSceneName = _curSceneName;

			// 该场景无系统创建
			if (!_sysDic.TryGetValue(sceneName, out BaseScene scene))
			{
				scene = SceneFactory.Instance.Create<T>(args);
				_sysDic.Add(sceneName, scene);
			}
			else
			{
				// 场景类型改变了
				if (!( scene is T ))
				{
					scene.Dispose();
					var newScene = SceneFactory.Instance.Create<T>(args);
					_sysDic[sceneName] = newScene;
				}
				else
				{
					// 重置
					scene.Reset();
				}
			}

			// TODO 加载时的切换

			LoadScene(sceneName, () => 
			{
				onComplate?.Invoke();
				UnLoadScene(unLoadSceneName);
			});
		}

		// 场景切换 直接加载场景 隐藏但不会卸载上个场景 
		public void SwitchSceneFast<T> (string sceneName, ISceneArgs args = null, Action onComplate = null) where T : BaseScene, new ()
		{
			var displaySceneName = _curSceneName;

			// 该场景无系统创建
			if (!_sysDic.TryGetValue(sceneName, out BaseScene scene))
			{
				scene = SceneFactory.Instance.Create<T>(args);
				_sysDic.Add(sceneName, scene);
			}
			else
			{
				// 场景类型改变了
				if (!( scene is T ))
				{
					scene.Dispose();
					var newScene = SceneFactory.Instance.Create<T>(args);
					_sysDic[sceneName] = newScene;
				}
				else
				{
					// 重置
					scene.Reset();
				}
			}

			// 隐藏场景
			DisplayScene(displaySceneName);

			// TODO 加载时的切换
			LoadScene(sceneName, () => 
			{
				onComplate?.Invoke();

				UnityEngine.Resources.UnloadUnusedAssets();
			});
		}

		// 场景加载
		public void LoadScene (string sceneName, Action onComplate = null)
		{
			if (sceneName == null)
			{
				return;
			}

			// 场景正在加载
			if (_asyncUpdateProgressHandle != null)
			{
				return;
			}

			// 获取加载缓存
			if (!_sceneLoaderCache.TryGetValue(sceneName, out SceneLoader sceneLoader))
			{
				sceneLoader = new SceneLoader(sceneName);
				// 当前为默认场景 卸载默认场景
				if (_curSceneName == _defaultScene)
				{
					sceneLoader.LoadScene(LoadSceneMode.Single);
				}
				else
				{
					sceneLoader.LoadScene(LoadSceneMode.Additive);
				}

				_sceneLoaderCache.Add(sceneName, sceneLoader);
			}

			var preSceneName = _curSceneName;

			_curSceneName = sceneName;

			var handle = sceneLoader.Handle;

			if (!handle.IsValid())
			{
				return;
			}

			// 更新记录场景
			UpdateLastScene(_curSceneName);
			
			// 未加载完成
			if (!handle.IsDone)
			{
				_asyncUpdateProgressHandle = StartCoroutine(UpdateProgress(sceneName, onComplate));
			}
			else // 加载完成
			{
				var activeHandle = handle.Result.ActivateAsync();
				_asyncUpdateProgressHandle = StartCoroutine(UpdateProgress(sceneName, activeHandle, onComplate));
			}
		}
		
		private IEnumerator UpdateProgress (string sceneName, Action action)
		{

			var handle = GetSceneLoader(sceneName).Handle;

			while (!handle.IsDone)
			{
				_loadProgress = handle.GetDownloadStatus().Percent;
				// TODO 进度更新事件

				yield return null;
			}

			_loadProgress = 1;

			if (handle.Status == AsyncOperationStatus.Failed)
			{
#if UNITY_EDITOR
				Debug.LogError("场景加载失败");
# endif
				yield break;
			}

			// 加载成功
			yield return new WaitForSeconds(0.5f);
			// 过一段时间激活
			 AsyncOperation asyncHandle = handle.Result.ActivateAsync();

			yield return UpdateProgress(sceneName, asyncHandle, action);
		}

		private IEnumerator UpdateProgress (string sceneName, AsyncOperation handle, Action action)
		{
			SceneLoader loader = null;
			if (sceneName != _defaultScene)
			{
				loader = GetSceneLoader(sceneName);
			}

			while (!handle.isDone)
			{
				_loadProgress = handle.progress;
				// TODO 进度更新事件

				yield return null;
			}

			_loadProgress = 1;

			yield return new WaitForSeconds(0.5f);

			handle.allowSceneActivation = true;

			handle.completed += (result) =>
			{
				// 更新检查缩容
				UpdateShrink(_cacheCapacity);

				// TODO 考虑缓存
				if (loader != null)
				{
					UnityEngine.SceneManagement.SceneManager.SetActiveScene(loader.Handle.Result.Scene);
				}

				OnLoadScene(CurrentSceneName);
				action?.Invoke();
			};

			// 加载完成
			if (_asyncUpdateProgressHandle != null)
			{
				StopCoroutine(_asyncUpdateProgressHandle);
				_asyncUpdateProgressHandle = null;
			}
		}

		// 加载完成
		private void OnLoadScene (string sceneName)
		{
			var scene = GetSceneSystem(sceneName);
			if (scene == null)
			{
#if UNITY_EDITOR
				Debug.LogError("该场景缺失场景系统");
#endif
				return;
			}

			scene.OnLoadScene();
			scene.ActiveScene(true);
		}

		// 卸载场景
		public void UnLoadScene (string sceneName)
		{
			if (sceneName == null)
			{
				return;
			}

			var scene = GetSceneSystem(sceneName);
			if (scene != null)
			{
				scene.Dispose();
				return;
			}

			ReleaseScene(sceneName);
		}

		// 隐藏场景
		public void DisplayScene (string sceneName)
		{
			if (sceneName == null)
			{
				return;
			}

			// 销毁场景系统 TODO 异步优化
			var curSceneSys = GetSceneSystem(sceneName);
			if (curSceneSys != null)
			{
				curSceneSys.ActiveScene(false);
				curSceneSys.Dispose();
			}
		}

		// 更新最新的场景记录
		private void UpdateLastScene (string sceneName)
		{
			LinkedListNode<string> node = null;

			// 默认场景无缓存
			if (sceneName == _defaultScene)
			{
				return;
			}

			if (!_lruDic.TryGetValue(sceneName, out node))
			{
				node = new LinkedListNode<string>(sceneName);
				_lruDic.Add(sceneName, node);
				_lruCache.AddFirst(node);
			}

			// 有缓存但是场景被释放
			if (node == null)
			{
				node = new LinkedListNode<string>(sceneName);
				_lruDic[ sceneName] = node;
				_lruCache.AddFirst(node);
			}

			// 更新最新打开到头部
			_lruCache.Remove(node);
			_lruCache.AddFirst(node);
		}

		private void UpdateShrink (int maxCount)
		{
			if (_lruCache.Count <= maxCount)
			{
				return;
			}

			var curNode = _lruCache.Last;
			// 清除冗余
			while (_lruCache.Count > maxCount && curNode.Value != CurrentSceneName && curNode != null)
			{
				UnLoadScene(curNode.Value);
				curNode = curNode.Previous;
			}
		}

		// 释放场景
		private void ReleaseScene (string sceneName)
		{
			if (!_sceneLoaderCache.TryGetValue(sceneName, out SceneLoader sceneLoader))
			{
				return;
			}
			_sceneLoaderCache.Remove(sceneName);
			sceneLoader.Release();

			if (!_lruDic.TryGetValue(sceneName, out LinkedListNode<string> node))
			{
				return;
			}
			_lruDic[sceneName] = null;
			_lruCache.Remove(node);
		}

		private void ClearCache ()
		{
			foreach (var sceneName in _sceneLoaderCache.Keys)
			{
				var curSceneSys = GetSceneSystem(sceneName);
				if (curSceneSys != null)
				{
					curSceneSys.Dispose();
				}
			}

			_sceneLoaderCache.Clear();

		}
		
	}
}
