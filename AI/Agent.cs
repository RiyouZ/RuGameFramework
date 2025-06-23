using RuAI.HTN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  RuAI
{
	// AI代理体
	public class Agent : MonoBehaviour, IAIAgent
	{
		// AI逻辑驱动
		public IAIDriver aiRunner;

		public Dictionary<string, WorldSensor> LocalWorldState
		{
			get; set;
		}

		public BaseTask root;

	}
}

