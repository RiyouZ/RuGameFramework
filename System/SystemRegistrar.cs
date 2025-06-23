using RuGameFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.System
{
	public static class SystemRegistrar
	{
		private static int DefaultCapacity = 32;
		private static Dictionary<SystemType, Type> _sysDic = new Dictionary<SystemType, Type>(DefaultCapacity);
		private static Dictionary<SystemType, IRuSystem> _sysCacheDic = new Dictionary<SystemType, IRuSystem>(DefaultCapacity);

		private static Type GetSystemType (SystemType sysType)
		{
			if (!_sysDic.TryGetValue(sysType, out Type type))
			{
				return null;
			}
			return type;
		}

		public static void RegisterSystemType<T> (SystemType sysType)
		{
			if (_sysDic.ContainsKey(sysType))
			{
				return;
			}
			_sysDic.Add(sysType, typeof(T));
		}

		public static void UnRegisterSystemType (SystemType sysType)
		{
			if (!_sysDic.ContainsKey(sysType))
			{
				return;
			}

			_sysDic.Remove(sysType);
		}

		public static IRuSystem Create (SystemType sysType)
		{
			Type type = GetSystemType(sysType);

			if (type == null)
			{
#if UNITY_EDITOR
				Debug.LogError($"System Type {sysType} Î´×¢²á");
#endif
				var nullSys = new NullSystem();
				return nullSys;
			}

			if (!_sysCacheDic.TryGetValue(sysType, out IRuSystem system))
			{
				system = Activator.CreateInstance(type) as IRuSystem;
				_sysCacheDic.Add(sysType, system);
			}

			return system;
		}

	}
}