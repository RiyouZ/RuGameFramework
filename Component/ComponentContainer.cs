using RuGameFramework.Core;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Component
{
	public class ComponentContainer
	{
		private Dictionary<ComponentType, IRuComponent> _comDic;

		public ComponentContainer ()
		{
			_comDic = new Dictionary<ComponentType, IRuComponent>();
		}

		public ComponentContainer (int maxCapacity)
		{
			_comDic = new Dictionary<ComponentType, IRuComponent>(maxCapacity);
		}

		public IRuComponent GetComponent (ComponentType type)
		{
			if (!_comDic.TryGetValue(type, out IRuComponent com))
			{
				return new NullCom();
			}

			return com;
		}

		public IRuComponent AddComponent (ComponentType comType, GameObject bindObj)
		{
			if (_comDic.ContainsKey(comType))
			{
				return new NullCom();
			}

			IRuComponent com = ComponentRegistrar.Create(comType, bindObj);
			com.Init();
			_comDic.Add(comType, com);

			return com;
		}

		public void RemoveComponent (ComponentType comType)
		{
			if (!_comDic.TryGetValue(comType, out IRuComponent com))
			{
				return;
			}
			com.Dispose();
			_comDic.Remove(comType);
		}

		public void Release ()
		{
			if (_comDic == null)
			{
				return;
			}

			_comDic.Clear();
			_comDic = null;
		}

	}
}

