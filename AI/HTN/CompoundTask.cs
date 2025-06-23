using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace RuAI.HTN
{
	public class CompoundTask : BaseTask
	{
		public List<Method> methodList = new List<Method>();
		public Method CurrentMethod
		{
			get; private set;
		}

		public int CurrentMethodIndex
		{
			get; set;
		}

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
		
		// ��������ѡ������
		public override bool Condition (Dictionary<string, WorldSensor> worldState)
		{
			var copyWorldState = HTNSensors.CloneWorldState(worldState);
			// �����ʧ�� �����һ�γɹ��ĵط���ʼ����
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

