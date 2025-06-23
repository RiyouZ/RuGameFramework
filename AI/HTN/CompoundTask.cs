using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace RuAI.HTN
{
	public class CompoundTask : BaseTask
	{
		[InlineEditor]
		public List<Method> methodList = new List<Method>();
		public Method CurrentMethod
		{
			get; private set;
		}

		public int CurrentMethodIndex
		{
			get; set;
		}

		// 继承SO类创建
		public static T Create<T> (string name, Agent agent) where T : CompoundTask
		{
			var task = CreateInstance<T>();
			task.taskName = name;
			task.CharacterAgent = agent;
			task.CurrentMethodIndex = 0;
			return task;
		}

		public static CompoundTask Create (string name, Agent agent, List<Method> methods)
		{
			var task = Create<CompoundTask>(name, agent);
			task.methodList = methods;
			return task;
		}

		public static CompoundTask Create (string name, Agent agent)
		{
			return Create<CompoundTask>(name, agent);
		}

		public override void Initialize (IHTNTask parent)
		{
			this.parent = parent as BaseTask;
			foreach (var method in methodList)
			{
				method.Initialize(this);
			}
		}

		// 作为root节点 直接设置Agent
		public void InitializeAgent (Agent agent)
		{
			this.CharacterAgent = agent;
			foreach (var method in methodList)
			{
				method.Initialize(this);
			}
		}

		// 复合任务选择条件
		public override bool Condition (Dictionary<string, WorldSensor> worldState)
		{
			var copyWorldState = HTNSensors.CloneWorldState(worldState);
			// 如果子失败 则从上一次成功的地方开始迭代
			for (int i = CurrentMethodIndex; i < methodList.Count; i++)
			{
				var method = methodList[i];
				if (method.Condition(copyWorldState))
				{
					CurrentMethod = method;
					CurrentMethodIndex = i;
					return true;
				}
			}
			return false;
		}

		public void AddMethod (Method method)
		{
			methodList.Add(method);
		}

	}

}

