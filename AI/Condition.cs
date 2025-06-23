using RuAI.HTN;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI
{
	public abstract class Condition : ScriptableObject, IAICondition
	{
		public abstract bool IsTrue (Dictionary<string, WorldSensor> worldSensor);
	}

	public class AnoCondition : Condition
	{
		private Func<Dictionary<string, WorldSensor>, bool> _func;

        public void SetCondition(Func<Dictionary<string, WorldSensor>, bool> func)
        {
            _func = func;
        }

        public override bool IsTrue (Dictionary<string, WorldSensor> worldSensor)
		{
			return _func(worldSensor);
		}

		
	}

}
