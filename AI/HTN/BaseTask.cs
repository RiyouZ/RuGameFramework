using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI.HTN
{
	public abstract class BaseTask : ScriptableObject, IHTNTask
	{
		public string taskName;
		public string TaskName => taskName;

		public Agent CharacterAgent
		{
			get; set;
		}

		public TaskStatus Status
		{
			get; private set;
		}

		// 与运算
		protected List<IAICondition> _conditions = new List<IAICondition>();

		public void AddCondition (IAICondition condition)
		{
			if (_conditions == null)
			{
				_conditions = new List<IAICondition>();
			}

			_conditions.Add(condition);
		}

		public void AddCondition (Func<Dictionary<string, WorldSensor>, bool> func)
		{
			var condition = ScriptableObject.CreateInstance<AnoCondition>();
			condition.SetCondition(func);
			_conditions.Add(condition);
		}

		// 判断条件 规划/运行
		public virtual bool Condition (Dictionary<string, WorldSensor> worldState)
		{
			foreach (var condition in _conditions)
			{
				if (!condition.IsTrue(worldState))
				{
					return false;
				}
			}
			return true;
		}
		public virtual void Plan (Dictionary<string, WorldSensor> worldState) { }

		public virtual IEnumerator Run () {yield return null;}
	}

}
