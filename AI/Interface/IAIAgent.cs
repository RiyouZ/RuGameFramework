using RuAI.HTN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI
{
	public interface IAIAgent
	{
		public Dictionary<string, WorldSensor> LocalWorldState
		{
			get; set;
		}
	}

}
