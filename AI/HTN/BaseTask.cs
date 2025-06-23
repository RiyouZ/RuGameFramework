using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI.HTN
{
	public abstract class BaseTask : ScriptableObject, ISerializationCallbackReceiver, IHTNTask
	{
		[SerializeField, HideInInspector]
		private SerializationData serializationData = new SerializationData();

		protected BaseTask parent;

		public string taskName;
		public string TaskName => taskName;

		private Agent _agent;
		public Agent CharacterAgent
		{
			get
			{
				if (_agent == null && parent != null)
				{
					_agent = parent.CharacterAgent;
				}
				return _agent;
			}
			set
			{
				_agent = value;
			}
		}

		public TaskStatus Status
		{
			get; private set;
		}

		// 与运算
		[ShowInInspector]
		protected List<ScriptableCondition> conditions = new List<ScriptableCondition>();

		public void AddCondition (ScriptableCondition condition)
		{
			if (conditions == null)
			{
				conditions = new List<ScriptableCondition>();
			}

			conditions.Add(condition as ScriptableCondition);
		}

		public void AddCondition (Func<Dictionary<string, WorldSensor>, bool> func)
		{
			var condition = ScriptableObject.CreateInstance<AnoCondition>();
			condition.SetCondition(func);
			conditions.Add(condition);
		}

		// 判断条件 规划/运行
		public virtual bool Condition (Dictionary<string, WorldSensor> worldState)
		{
			foreach (var condition in conditions)
			{
				if (!condition.IsTrue(worldState))
				{
					return false;
				}
			}

			return true;
		}

		public void OnBeforeSerialize ()
		{
			UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
		}

		public void OnAfterDeserialize ()
		{
			UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
		}

		public virtual void Initialize (IHTNTask parent) { }
	}

}
