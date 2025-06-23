using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuAI.HTN
{
	// 世界状态
	public class WorldSensor
	{
		public object value;
		// TODO 封装设置
		public Action<object> setter;
		public Func<object, object> getter;

		public void Set<T> (T value)
		{
			this.value = value;
			setter?.Invoke (value);
		}

		public T Get<T> ()
		{
			if (getter == null)
			{
				return (T)value;
			}

			return (T)getter?.Invoke(value);
		}

		public WorldSensor Clone ()
		{
			WorldSensor copy = new WorldSensor ();
			copy.value = value;
			copy.setter = setter;
			copy.getter = getter;
			return copy;
		}
	}

	public static class HTNSensors
	{
		private static Dictionary<string, WorldSensor> _worldState = new Dictionary<string, WorldSensor>();

		public static Dictionary<string, WorldSensor> WorldState => _worldState;


		public static Dictionary<string, WorldSensor> CloneWorldState (Dictionary<string, WorldSensor> origin)
		{
			return origin.ToDictionary(origin => origin.Key, origin => origin.Value.Clone());
		}


	}
}

