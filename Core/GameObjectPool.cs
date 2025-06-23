using RuGameFramework.Assets;
using RuGameFramework.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RuGameFramework.Mono;
using UnityEngine;

namespace RuGameFramework.Core
{
	public class GameObjectPool : IObjectPool<GameObject>
	{
		private const string PoolName = "Pool";
		private const int MultiExpansion = 2;
		private string _objPath;

		private int _count;
		private int _capacity;

		private TimerManager _timerMgr;

		private Transform _objPoolTs;
		private GameObject _prefab;
		private List<GameObject> _freeList;
		private float _shrinkTime;
		private float _updateShrinkTime;

		private AutoIdGenerator _autoIdGenerator;

		// TODO 缩容时候的锁
		private bool _checkLock;
		private bool _isPreload;
		public bool IsPreload => _isPreload;

		private Timer _checkTimer;
		private float _timeSimple;
		
		// 加载句柄
		private static Dictionary<int, Coroutine> _asyncLoadHandleDic;

		private bool _isNotAssetLoad;

		private Action<GameObject> _initAction;

		public Action<GameObject> InitAction
		{
			set
			{
				_initAction = value;
			}
		}

		private Action<GameObject> _collAction;
		public Action<GameObject> CollAction
		{
			set
			{
				_collAction = value;
			}
		}

		public GameObjectPool (GameObject prefab, Transform parent = null, int capacity = 2, bool canShrink = false, float timeSimple = 0.3f, float updateShrinkTime = 10f)
		{
			_prefab = prefab;
			_autoIdGenerator = new AutoIdGenerator(0);
			_checkLock = false;
			_capacity = capacity;
			_count = 0;
			_freeList = new List<GameObject>(_capacity);
			
			_isPreload = true;
			
			if (parent == null)
			{
				string poolName = String.Format("{0}_{1}", prefab.name, PoolName);
				_objPoolTs = new GameObject(poolName).transform;
			}
			else
			{
				_objPoolTs = parent;
			}

			for (int i = 0; i < _capacity; i++)
			{
				int autoId = _autoIdGenerator.GetAutoId();
				var obj = CreateObj(autoId, _objPath);
				_collAction?.Invoke(obj);
				_freeList.Add(obj);
			}

			_count = _freeList.Count;

			if (!canShrink)
			{
				return;
			}

			_updateShrinkTime = updateShrinkTime;
			_shrinkTime = updateShrinkTime;
			_timeSimple = timeSimple;

			// 缩容计时
			_timerMgr = App.Instance.TimerManager;
			_checkTimer = _timerMgr.SetInterval(Update, _timeSimple, 0);

		}
		
		public GameObjectPool(string objPath, Transform parent = null, int capacity = 2, bool canShrink = false, float timeSimple = 0.3f, float updateShrinkTime = 10f)
		{
			_objPath = objPath;
			_autoIdGenerator = new AutoIdGenerator(0);
			_checkLock = false;
			_capacity = capacity;
			_count = 0;
			_freeList = new List<GameObject>(_capacity);
			_isPreload = false;
			_asyncLoadHandleDic = new Dictionary<int, Coroutine>(_capacity);

			if (parent == null)
			{
				_objPoolTs = new GameObject(PoolName).transform;
			}
			else
			{
				_objPoolTs = parent;
			}

			for (int i = 0; i < _capacity; i++)
			{
				int autoId = _autoIdGenerator.GetAutoId();
				CreateObjAsync(autoId, _objPath, (obj) =>
				{
					_collAction?.Invoke(obj);
					_freeList.Add(obj);
					
					_count++;
					// 预加载完毕
					if (_count == _capacity)
					{
						_isPreload = true;
					}
				});
			}
			
			// AssetsManager.LoadAsset<GameObject>(_objPath, (prefab) =>
			// {
			// 	_prefab = prefab;
			//
			// 	for (int i = 0; i < _capacity; i++)
			// 	{
			// 		int autoId = _autoIdGenerator.GetAutoId();
			// 		var obj = CreateObj(autoId, _prefab);					
			// 		_collAction?.Invoke(obj);
			// 		_freeList.Add(obj);
			// 	}
			// 	_count = _freeList.Count;
			// 	_isPreload = true;
			// });

			if (!canShrink)
			{
				return;
			}

			_updateShrinkTime = updateShrinkTime;
			_shrinkTime = updateShrinkTime;
			_timeSimple = timeSimple;

			// 缩容计时
			_timerMgr = App.Instance.TimerManager;
			_checkTimer = _timerMgr.SetInterval(Update, _timeSimple, 0);
		}

		private void Update (float deltaTime)
		{
			_updateShrinkTime -= deltaTime;
			if (_updateShrinkTime <= 0)
			{
				_updateShrinkTime = _shrinkTime;
				Shrink();
			}
		}

		public void Collection (GameObject obj)
		{
			obj.SetActive(false);
			obj.transform.parent = _objPoolTs;
			_collAction?.Invoke(obj);

			_freeList.Add(obj);
			_count++;
		}

		public void Dispose ()
		{
			if (_checkTimer != null)
			{
				_checkTimer.Release();
			}

			foreach (var obj in _freeList)
			{
				AssetsManager.Destroy(obj);
			}

			_freeList.Clear();
			_freeList = null;

			_initAction = null;
			_collAction = null;

			_autoIdGenerator = null;
		}

		public IEnumerator WaitPoolLoad ()
		{
			while(!IsPreload) yield return null;
		}

		public GameObject Spawn ()
		{
			if (!_isPreload)
			{
				return null;
			}

			_updateShrinkTime = _shrinkTime;
			
			GameObject obj = null;
			if (_count <= 0)
			{
				int preCapacity = _capacity;
				_capacity = Mathf.Max(1, _capacity * MultiExpansion);

				for (int i = preCapacity; i < _capacity; i++)
				{
					int autoId = _autoIdGenerator.GetAutoId();
					GameObject newObj = null;
					if (_isNotAssetLoad)
					{	
						newObj = CreateObjNotCounter(autoId, _prefab);
					}
					else
					{
						newObj = CreateObj(autoId, _objPath);
					}
					
					_freeList.Add(newObj);
					_count++;
				}
			}

			obj = _freeList[_count - 1];
			_freeList.RemoveAt(_count - 1);
			_initAction?.Invoke(obj);
			obj.SetActive(true);

			_count--;
			return obj;
		}

		// 2倍缩容
		public void Shrink ()
		{
			if (_count <=  _capacity / MultiExpansion)
			{
				return;
			}

			_capacity = (int) _capacity / MultiExpansion;

			int shrinkCount = 0;
			int shrinkIndex = _count / MultiExpansion;
			for (int i = _freeList.Count - 1; i >= shrinkIndex; i--)
			{
				//  计数销毁
				AssetsManager.Destroy(_freeList[i]);
				shrinkCount++;
			}
			_freeList.RemoveRange(shrinkIndex, shrinkCount);
			_count = _freeList.Count;
		}

		// 根据Prefab创建 不计数 不经过AssetsManager
		private GameObject CreateObjNotCounter (int autoId, GameObject prefab)
		{
			var obj = GameObject.Instantiate(prefab);
			obj.name = String.Format("{0}_{1}", prefab, autoId);
			obj.SetActive(false);
			obj.transform.parent = _objPoolTs;

			return obj;
		}

		private GameObject CreateObj(int autoId, string prefab)
		{
			var obj = AssetsManager.Instantiate(prefab);
			obj.name = String.Format("{0}_{1}", prefab, autoId);
			obj.SetActive(false);
			obj.transform.parent = _objPoolTs;

			return obj;
		}

		private void CreateObjAsync (int autoId, string prefab, Action<GameObject> onCreate)
		{
			GameObject obj = null;
			
			if (!_asyncLoadHandleDic.ContainsKey(autoId))
			{
				_asyncLoadHandleDic.Add(autoId, null);
			}
			
			//  计数实例化
			_asyncLoadHandleDic[autoId] = App.Instance.StartCoroutine(AssetsManager.AsyncInstantiate(_objPath, (gameObject) =>
			{
				gameObject.name = String.Format("{0}_{1}", prefab, autoId);
				gameObject.SetActive(false);
				gameObject.transform.parent = _objPoolTs;
				
				onCreate?.Invoke(gameObject);
				
				if (_asyncLoadHandleDic[autoId] != null)
				{
					App.Instance.StopCoroutine(_asyncLoadHandleDic[autoId]);
					_asyncLoadHandleDic[autoId] = null;
				}
			}));
		}
	}

}


