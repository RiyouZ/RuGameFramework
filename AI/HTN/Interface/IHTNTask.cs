using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI.HTN
{
	public enum TaskStatus
	{
		Success,
		Running,
		Failure
	}

	public interface IHTNTask
	{
		public Agent CharacterAgent { get; }
		
		public TaskStatus Status 
		{
			get;
		}

		public string TaskName
		{
			get;
		}

		public bool Condition(Dictionary<string, WorldSensor> worldState);

		// ¹æ»®
		public void Plan (Dictionary<string, WorldSensor> worldState);
	}
}

