using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI.HTN
{
	public class Method : BaseTask
	{
		[InlineEditor]
		public List<BaseTask> subTask = new List<BaseTask>();

		public static Method Create (string name, Agent agent, Func<Dictionary<string, WorldSensor>, bool> condition, List<BaseTask> subTasks)
		{
			var method = Create<Method>(name, agent);
			method.subTask = subTasks;
			method.AddCondition(condition);
			return method;
		}

		public static Method Create (string name, Agent agent)
		{
			return Create<Method>(name, agent);
		}

		public static T Create<T> (string name, Agent agent) where T : Method
		{
			var method = CreateInstance<T>();
			method.taskName = name;
			method.CharacterAgent = agent;
			return method;
		}

		public override void Initialize (IHTNTask parent)
		{
			this.parent = parent as BaseTask;
			foreach (var task in subTask)
			{
				task.Initialize(this);
			}
		}

		public void AddSubTask (IHTNTask task)
		{
			subTask.Add(task as BaseTask);
		}
	}

}
