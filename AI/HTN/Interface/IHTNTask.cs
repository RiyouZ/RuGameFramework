using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI.HTN
{
	public enum TaskStatus
	{
		None,
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
		public void Initialize (IHTNTask parent);

		public bool Condition(Dictionary<string, WorldSensor> worldState);
	}
}

