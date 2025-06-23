using RuGameFramework;
using RuGameFramework.Assets;
using RuGameFramework.Core;
using RuGameFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RuUI
{
	public static readonly int ErrorOrder = -1;
	public static readonly int DefaultCapacity = 4;
	// ������Canvasջ
	private static Stack<IRuCanvas> _canvasStk;
	// Canvas����
	private static Dictionary<string, IRuCanvas> _canvasCache;

	private class CanvasLayerInfo
	{
		public Canvas canvas;
		public int uiOrder;

		public CanvasLayerInfo()
		{
			canvas = null;
			uiOrder = 0;
		}
	}

	private static Dictionary<string, CanvasLayerInfo> _canvasLayerMapping = new Dictionary<string, CanvasLayerInfo>
		{	
			{UISortLayer.Scene, new CanvasLayerInfo()},
			{UISortLayer.Hud, new CanvasLayerInfo()},
			{UISortLayer.Window, new CanvasLayerInfo()},
			{UISortLayer.Effect, new CanvasLayerInfo()},
			{UISortLayer.Cursor, new CanvasLayerInfo()}
		};
	
	// �����㼶�ĸ��ڵ�
	private static Canvas _hudCanvas;
	private static Canvas _sceneCanvas;
	private static Canvas _windowCanvas;
	private static Canvas _effectCanvas;
	private static Canvas _cursorCanvas;
	private static Canvas _loadCanvas;

	// �����㼶��ǰ��UIOrder
	private static int _hudOrder = 0;
	private static int _sceneOrder = 0;
	private static int _windowOrder = 0;
	private static int _effectOrder = 0;
	private static int _cursorOrder = 0;

	// UI��Ⱦ���
	private static Camera _uiCamera;

	// ͬʱ���ص�Canvas����
	private static int _multiLoad;
	private static Dictionary<string, Coroutine> _asyncLoadHandleDic;
	
	// ����������
	private static IAsyncAssetLoadAdapter _loadAdapter;

	public static void CreateGameUI(IAsyncAssetLoadAdapter assetLoadAdapter, string gameUIPath, Action<GameObject> onCreate = null, int multiLoad = 2)
	{
		if (assetLoadAdapter == null)
		{
			return;
		}
		
		if (_loadAdapter == null)
		{
			_loadAdapter = assetLoadAdapter;
		}
		
		_canvasStk = new Stack<IRuCanvas>(DefaultCapacity);
		_canvasCache = new Dictionary<string, IRuCanvas>(DefaultCapacity);
		_multiLoad = multiLoad;

		if (_asyncLoadHandleDic == null)
		{
			_asyncLoadHandleDic = new Dictionary<string, Coroutine>(_multiLoad + 1);
		}

		if (!_asyncLoadHandleDic.ContainsKey(gameUIPath))
		{
			_asyncLoadHandleDic.Add(gameUIPath, null);
		}

		_asyncLoadHandleDic[gameUIPath] = _loadAdapter.AsyncLoadPrefab(gameUIPath, (gameUIObj) =>
		{
			OnCreateGameUI(gameUIObj, onCreate);
			if (_asyncLoadHandleDic != null)
			{
				_loadAdapter.StopCoroutine(_asyncLoadHandleDic[gameUIPath]);
				_asyncLoadHandleDic.Remove(gameUIPath);
			}
		});
	}
	
	private static void OnCreateGameUI (GameObject gameUIObj, Action<GameObject> onCreate = null)
	{
		var gameUITs = gameUIObj.GetComponent<Transform>();
		gameUITs.name = "GameUI";

		_canvasLayerMapping[UISortLayer.Hud].canvas = gameUITs.Find("HUD").GetComponent<Canvas>();
		_canvasLayerMapping[UISortLayer.Scene].canvas = gameUITs.Find("Scene").GetComponent<Canvas>();
		_canvasLayerMapping[UISortLayer.Window].canvas = gameUITs.Find("Window").GetComponent<Canvas>();
		_canvasLayerMapping[UISortLayer.Effect].canvas = gameUITs.Find("Effect").GetComponent<Canvas>();
		_canvasLayerMapping[UISortLayer.Cursor].canvas = gameUITs.Find("Cursor").GetComponent<Canvas>();
		_loadCanvas = gameUITs.Find("Loading").GetComponent<Canvas>();
		_uiCamera = gameUITs.Find("UICamera").GetComponent<Camera>();

		onCreate?.Invoke(gameUIObj);
	}

	// ��ʾUI
	public static void ShowCanvas (Action<IRuCanvas> onShow, string uiPath, string uiLayer)
	{
		// ���ڻ���
		if (_canvasCache.TryGetValue(uiPath, out IRuCanvas ruCanvas))
		{
			ruCanvas.UIReset();
			ruCanvas.ShowFast();
			UpdateUIParent(ruCanvas);
			MarkCanvasOpen(ruCanvas);
			onShow(ruCanvas);
			return;
		}

		if (!_asyncLoadHandleDic.ContainsKey(uiPath) && _asyncLoadHandleDic.Count <= _multiLoad)
		{
			_asyncLoadHandleDic.Add(uiPath, null);
		}
		
		Transform parentTs = null;
		if (uiLayer != null)
		{
			parentTs = GetUIParentByUILayer(uiLayer);
		}
		
		_asyncLoadHandleDic[uiPath] = _loadAdapter.AsyncLoadPrefab(uiPath, (prefab) =>
		{
			//  ʵ�����ü���
			GameObject canvas = prefab;
			
			var canvasCom = canvas.GetComponent<IRuCanvas>();
			if (canvasCom == null)
			{
				return;
			}
			
			OnCanvasLoad(canvasCom, uiPath, uiLayer);
			onShow(canvasCom);
			
			if (_asyncLoadHandleDic != null)
			{
				_loadAdapter.StopCoroutine(_asyncLoadHandleDic[uiPath]);
				_asyncLoadHandleDic.Remove(uiPath);
			}
			// Ϊ����Prefabһ�� ʵ����ʱ����Ҫ����parent
		}, parentTs);
	}

	public static void ShowCanvas (IRuCanvas ruCanvas)
	{
		var canvas = GetCanvasCache(ruCanvas.CanvasPath);
		if (canvas == null)
		{
#if UNITY_EDITOR
			Debug.LogWarning($"[RuUI] Canvas cache not found: {ruCanvas.CanvasPath}]");
#endif
			return;
		}

		canvas.UIReset();
		canvas.ShowFast();
	}

	private static void OnCanvasLoad (IRuCanvas ruCanvas, string canvasPath, string uiLayer = null)
	{
		if (ruCanvas == null)
		{
			return;
		}

		if (uiLayer != null)
		{
			ruCanvas.UILayer = uiLayer;
			Debug.LogWarning($"[RuUI] OnCanvasLoad {canvasPath} UILayer: {ruCanvas.UILayer}");
		}

		if (ruCanvas.CanvasPath == null)
		{
			ruCanvas.CanvasPath = canvasPath;
		}

		// ����Order
		int order = GetUIOrderByUILayer(ruCanvas.UILayer);
		ruCanvas.UIOrder = order;

		// ��ӻ���
		AddCanvasCache(canvasPath, ruCanvas);
		// ��ӵ��򿪵�UIջ
		MarkCanvasOpen(ruCanvas);
		// ��һ�δ�
		ruCanvas.Show();
	}

	private static void InitCanvasHandle (Canvas canvas)
	{
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = _uiCamera;
	}

	// ���CanvasΪ��
	private static void MarkCanvasOpen (IRuCanvas canvas)
	{
		if (canvas == null)
		{
			return;
		}
		
		// ջ����Canvas���Ի���
		canvas.IsInteractable = true;
		if (_canvasStk.Count > 0)
		{
			var topCanvas = _canvasStk.Peek();
			topCanvas.IsInteractable = false;
		}
		_canvasStk.Push(canvas);
	}

	// �ر����ϲ�Canvas
	public static void CloseTopCanvas (bool isFast = false)
	{
		if (_canvasStk.Count <= 0)
		{
			return;
		}

		var topCanvas = _canvasStk.Pop();
		topCanvas.IsInteractable = false;

		DecreaseUIOrderByUILayer(topCanvas.UILayer);

		HideCanvasHandle(topCanvas, isFast);
	}

	private static void AddCanvasCache (string canvasPath, IRuCanvas canvas)
	{
		if (_canvasCache.ContainsKey(canvasPath))
		{
			return;
		}
		canvas.Init();
		_canvasCache.Add(canvasPath, canvas);
	}

	private static void RemoveCanvasCache (string canvasPath)
	{
		if (!_canvasCache.TryGetValue(canvasPath, out IRuCanvas canvas))
		{
			return;
		}
		canvas.UIDestory();
		_canvasCache.Remove(canvasPath);
	}

	private static IRuCanvas GetCanvasCache (string canvasPath)
	{
		if (!_canvasCache.TryGetValue(canvasPath, out IRuCanvas canvas))
		{
			return null;
		}
		return canvas;
	}

	// ��ӵ���Ӧ��parent ����Order
	private static void UpdateUIParent (IRuCanvas canvas)
	{
		if (canvas == null)
		{
			return;
		}

		string layer = canvas.UILayer;
		Transform canvasTs = canvas.CanvasTs;
		
		if (canvasTs == null)
		{
			return;
		}

		Transform parent = GetUIParentByUILayer(layer);
		if (parent != null && canvasTs.parent != parent)
		{
			canvasTs.parent = parent;
		}

		int order = GetUIOrderByUILayer(layer);

		if (order != ErrorOrder)
		{
			canvas.UIOrder = order;
		}

	}

	private static Transform GetUIParentByUILayer (string uiLayer)
	{
		if (uiLayer == null)
		{
			return null;
		}

		if (!_canvasLayerMapping.TryGetValue(uiLayer, out var canvasInfo))
		{
#if UNITY_EDITOR
			Debug.LogError($"[RuUI] {uiLayer} û������");
#endif
			return null;
		}

		return canvasInfo.canvas.transform;
	}

	private static int GetUIOrderByUILayer (string uiLayer)
	{
		if (uiLayer == null)
		{
			return ErrorOrder;
		}

		if (!_canvasLayerMapping.TryGetValue(uiLayer, out var canvasInfo))
		{
#if UNITY_EDITOR
			Debug.LogError($"[RuUI] {uiLayer} û������");
#endif
			return ErrorOrder;
		}

		canvasInfo.uiOrder++;
		
		return canvasInfo.uiOrder;
	}

	private static void DecreaseUIOrderByUILayer (string uiLayer)
	{
		if (uiLayer == null)
		{
			return;
		}
		
		if (!_canvasLayerMapping.TryGetValue(uiLayer, out var canvasInfo))
		{
#if UNITY_EDITOR
			Debug.LogError($"[RuUI] {uiLayer} û������");
#endif
			return;
		}

		canvasInfo.uiOrder--;
	}

	// ���ٵ�ǰ���е�Canvas ���ᴥ�������߼�
	public static void ClearAllCanvas ()
	{
		_canvasStk.Clear();
		_canvasStk = null;

		foreach (var canvas in _canvasCache.Values)
		{
			canvas.UIDestory();
			AssetsManager.Destroy(canvas.UIGameObject);
		}
		_canvasCache.Clear();
	}
	
	// ����UI
	public static void HideCanvas (string canvasPath, bool isFast = false)
	{
		if (!_canvasCache.TryGetValue(canvasPath, out IRuCanvas canvas))
		{
			return;
		}

		// û�д򿪵� canvas
		var topCanvas = _canvasStk.Peek();
		if (topCanvas == canvas )
		{
			CloseTopCanvas(isFast);
			return;
		}

		HideCanvasHandle(canvas);
	}

	public static void HideCanvas (IRuCanvas canvas, bool isFast = false)
	{
		if (!_canvasCache.TryGetValue(canvas.CanvasPath, out IRuCanvas ruCanvas))
		{
			return;
		}
		HideCanvasHandle(canvas, isFast);
	}

	// ����UI����
	private static void HideCanvasHandle (IRuCanvas canvas, bool  isFast = false)
	{
		if (!isFast)
		{
			canvas.Hide();
			RemoveCanvasCache(canvas.CanvasPath);
			// �ͷ�Canvas
			AssetsManager.Destroy(canvas.UIGameObject);
		}
		else
		{
			canvas.HideFast();
		}
	}
}
