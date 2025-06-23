using RuAI.HTN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI
{
	public interface IAICondition
	{
		public bool IsTrue (Dictionary<string, WorldSensor> worldSensorMap);
	}

}
