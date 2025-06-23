using RuGameFramework.Assets;
using RuGameFramework.Config;
using RuGameFramework.Input;
using RuGameFramework.Core;
using System.Collections.Generic;
using UnityEngine;
using RuGameFramework.Scene;
using UnityEngine.AddressableAssets;
using Game.GamePlay.Global;
using Game.GamePlay.Operator;
using RuGameFramework.System;
using RuGameFramework.Mono;
using RuGameFramework.Component;
using Game.GamePlay.Component;
using Game.GamePlay.Player;
using Game.GamePlay.Scene;
using Game.GamePlay.Scene.Data;

namespace RuGameFramework
{
	public class App : MonoBehaviour
	{
		private static string GameUIPath = "Assets/GameAssets/UI/GameUI.prefab";

		private static App _instance;
		public static App Instance => _instance;

		private GameMono _gameMono;

		private MouseManager _mouseManager;
		public MouseManager MouseManager => _mouseManager;

		private ConfigManager _configManager;
		public ConfigManager ConfigManager => _configManager;

		private TimerManager _timerManager;
		public TimerManager TimerManager => _timerManager;

		private SceneManager _sceneManager;
		public SceneManager SceneManager => _sceneManager;

		private Dictionary<int, GameObject> _dontDestroyDic = new Dictionary<int, GameObject>(8);

		private Timer timer;

		void Awake ()
		{
			if (_instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				_instance = this;
			}

			DontDestroyOnLoad(this);

			Application.targetFrameRate = 60;
		}

		void Start ()
		{
			var handle = Addressables.InitializeAsync();
			handle.Completed += (result) =>
			{
				OnAddressableInit();
			};
			
		}

		// Addressable��ʼ�����
		public void OnAddressableInit ()
		{
			InitManager();
			// ����UI
			RuUI.CreateGameUI(new AAAssetLoadAdapter(this), GameUIPath, OnCreateGameUI);
			RegisterSystem();
			RegisterComponent();
			RegisterData();

			// ��ȡȫ������
			AssetsManager.LoadAsset<GameDataSO>(ConfigManager.GlobalDataPath, OnGlobalDataInit);
		}

		// UI�������
		private void OnCreateGameUI (GameObject uiObj)
		{
			// ������Ϸ�����ָ��
			var cursorObj = new GameObject("Cursor");
			AddDontDestroyList(cursorObj);
			AddDontDestroyList(uiObj);

			cursorObj.AddComponent<CursorCom>();
			var sortLayer = cursorObj.AddComponent<GameSortLayerCom>();
			sortLayer.SortLayerName = UISortLayer.Cursor;
			_mouseManager.Cursor = cursorObj;


			// TODO ��ʾ������
		}

		// ȫ�����ݳ�ʼ�����
		public void OnGlobalDataInit (GameDataSO gameData)
		{
			ConfigManager.globalData = gameData;

			TimerManager.StartSchedule(TimerManager.TimeType.RealTimeSinceStartUp, ConfigManager.AppFrameSample);

			var data = ConfigManager.GetSceneDataSO(1);
			
			GameStart();
			
			SceneManager.SwitchScene<BattleScene>(data.path, new BattleSceneArgs
			{
				sceneData = data,
				sceneMonsterList = new List<BattleSceneArgs.MonsterInfo>
				{
					new BattleSceneArgs.MonsterInfo
					{
						id = "1001",
						count = 3
					},
				}
			});
		}

		private void GameStart ()
		{

		}

		void Update ()
		{

		}

		private void InitManager ()
		{
			if (_gameMono == null)
			{
				_gameMono = gameObject.AddComponent<GameMono>();
			}

			if (_mouseManager == null)
			{
				_mouseManager = gameObject.AddComponent<MouseManager>();
			}

			if (_configManager == null)
			{
				_configManager = gameObject.AddComponent<ConfigManager>();
			}

			if (_timerManager == null)
			{
				_timerManager = gameObject.AddComponent<TimerManager>();
			}

			if (_sceneManager == null)
			{
				_sceneManager = gameObject.AddComponent<SceneManager>();
				_sceneManager.SetDefaultScene("Start");
			}

			gameObject.AddComponent<DebugMono>();
		}
		
		// ע��Type �� ��Ӧ��ϵͳ
		private void RegisterSystem ()
		{
			System.SystemRegistrar.RegisterSystemType<NullSystem>(SystemType.None);
			System.SystemRegistrar.RegisterSystemType<OperateSystem>(SystemType.OperateSystem);
		}

		private void UnRegisterSystem ()
		{
			System.SystemRegistrar.UnRegisterSystemType(SystemType.None);
			System.SystemRegistrar.UnRegisterSystemType(SystemType.OperateSystem);
		}

		// ע��Type �� ��Ӧ�����
		private void RegisterComponent ()
		{
			Component.ComponentRegistrar.RegisterComponentType<NullCom>(ComponentType.None);
			Component.ComponentRegistrar.RegisterComponentType<GameSortLayerCom>(ComponentType.SortingLayer);
		}
		private void UnRegisterComponent ()
		{
			Component.ComponentRegistrar.UnRegisterComponentType(ComponentType.None);
			Component.ComponentRegistrar.UnRegisterComponentType(ComponentType.SortingLayer);
		}

		private void RegisterData ()
		{
			Data.DataRegistrar.RegisterData<PlayerData>();
		}

		private void UnRegisterData ()
		{
			Data.DataRegistrar.UnRegisterData<PlayerData>();
		}

		public void AddDontDestroyList (GameObject obj)
		{
			if (obj == null)
			{
				return;
			}

			if (_dontDestroyDic.TryGetValue(obj.GetInstanceID(), out var dontDestroyObj))
			{
				return;
			}

			this._dontDestroyDic.Add(obj.GetInstanceID(), obj);	
			DontDestroyOnLoad(obj);
		}
		
		public void DestroyDontDestroyList (GameObject obj)
		{
			if (obj == null)
			{
				return;
			}
			
			if (!_dontDestroyDic.TryGetValue(obj.GetInstanceID(), out var dontDestroyObj))
			{
				return;
			}
			
			Destroy(obj);
			this._dontDestroyDic.Remove(obj.GetInstanceID());	
		}

		public void ClearDontDestroyList ()
		{
			if (this._dontDestroyDic == null)
			{
				return;
			}

			foreach (var obj in _dontDestroyDic.Values)
			{
				DestroyImmediate(obj);
			}

			this._dontDestroyDic.Clear();
			this._dontDestroyDic = null;
		}

	}

}
