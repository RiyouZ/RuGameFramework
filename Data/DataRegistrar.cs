using RuGameFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Data
{
	public static class DataRegistrar
	{
		private static int DefaultCapacity = 16;
		private static Dictionary<Type, IRuData> _dataDic = new Dictionary<Type, IRuData>(DefaultCapacity);

		private static Dictionary<Type, IObjectPool<IRuData>> _multiDataMap = new Dictionary<Type, IObjectPool<IRuData>>(DefaultCapacity); 

		public static void RegisterDataSingle (Type dataType)
		{
			if (_dataDic.TryGetValue(dataType, out IRuData data))
			{
				return;
			}

			var ruData = Activator.CreateInstance(dataType) as IRuData;
			ruData.Init();

			_dataDic.Add(dataType, ruData);
		}

		public static void RegisterDataSingleLazy (Type dataType)
		{
			if (_dataDic.TryGetValue(dataType, out IRuData data))
			{
				return;
			}

			_dataDic.Add(dataType, null);
		}

		public static void RegisterDataSingle<T> (bool isLazy = false) where T : IRuData
		{
			if (!isLazy)
			{
				RegisterDataSingle(typeof(T));
				return;
			}

			RegisterDataSingleLazy(typeof(T));
		}

		public static void UnRegisterDataSingle<T> () where T : IRuData
		{
			UnRegisterDataSingle(typeof(T));
		}

		public static void UnRegisterDataSingle (Type dataType)
		{
			if (!_dataDic.TryGetValue(dataType, out IRuData data))
			{
				return;
			}
			_dataDic.Remove(dataType);
		}

		public static IRuData GetDataSingle (Type dataType)
		{
			if (!_dataDic.TryGetValue(dataType, out IRuData data))
			{
#if UNITY_EDITOR
				Debug.LogError($"未注册类型 {dataType.Name}");
#endif
				return null;
			}

			return data;
		}

		public static T GetData<T> (bool isLazy = false) where T : class, IRuData
		{
			if (!isLazy)
			{
				return GetDataSingle(typeof(T)) as T;
			}
			return GetDataSingleLazy(typeof(T)) as T;
		}

		public static IRuData GetDataSingleLazy (Type dataType)
		{
			if (!_dataDic.TryGetValue(dataType, out IRuData data))
			{
#if UNITY_EDITOR
				Debug.LogError($"未注册类型 {dataType.Name}");
#endif
				return null;
			}

			if (data == null)
			{
				data = Activator.CreateInstance(dataType) as IRuData;
				data.Init();
				_dataDic[dataType] = data;
			}
			return data;
		}

		public static IRuData GetClone<T> (bool isLazy = false) where T : IRuData
		{
			if (!isLazy)
			{
				return GetClone(typeof(T));
			}

			return GetCloneLazy(typeof(T));
		}

		public static IRuData GetClone (Type dataType)
		{
			var data = GetDataSingle(dataType);
			if (data == null)
			{
				return null;
			}
			return data.Clone();
		}


		public static IRuData GetCloneLazy (Type dataType)
		{
			var data = GetDataSingleLazy(dataType);
			if (data == null)
			{
				return null;
			}

			return data.Clone();
		}
	}

}
