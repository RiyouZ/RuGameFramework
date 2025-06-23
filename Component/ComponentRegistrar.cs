using RuGameFramework.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Component
{
	public static class ComponentRegistrar
	{
		private static int DefaultCapacity = 32;
		private static Dictionary<ComponentType, Type> _sysDic = new Dictionary<ComponentType, Type>(DefaultCapacity);
		private static Dictionary<ComponentType, IRuComponent> _sysCacheDic = new Dictionary<ComponentType, IRuComponent>(DefaultCapacity);

		private static Type GetComponentType (ComponentType comType)
		{
			if (!_sysDic.TryGetValue(comType, out Type type))
			{
				return null;
			}
			return type;
		}

		public static void RegisterComponentType<T> (ComponentType comType)
		{
			if (_sysDic.ContainsKey(comType))
			{
				return;
			}
			_sysDic.Add(comType, typeof(T));
		}

		public static void UnRegisterComponentType (ComponentType comType)
		{
			if (!_sysDic.ContainsKey(comType))
			{
				return;
			}

			_sysDic.Remove(comType);
		}

		public static IRuComponent Create (ComponentType comType, GameObject bindObj)
		{
			Type type = GetComponentType(comType);

			if (type == null)
			{
#if UNITY_EDITOR
				Debug.LogError($"Component Type {comType} Î´×¢²á");
#endif
				var nullCom = bindObj.AddComponent(type) as IRuComponent;
				return nullCom;
			}

			if (!_sysCacheDic.TryGetValue(comType, out IRuComponent system))
			{
				system = bindObj.AddComponent(type) as IRuComponent;
				_sysCacheDic.Add(comType, system);
			}

			return system;
		}
	}
}
