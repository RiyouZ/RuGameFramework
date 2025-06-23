using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuAI.HTN
{
	public enum HTNState
	{
		None,
		Planning,
		Running
	}

	// 预演Task
	public class HTNDomain 
	{
		public HTNState DomainState => _htnState;

		private HTNState _htnState = HTNState.None;

		private CompoundTask _rootTask;

		private List<PrimitiveTask> _runTaskList;
		private HTNPlanner _plannner;
		private HTNRunner _runner;

		private Dictionary<string, WorldSensor> _worldState;

		public HTNDomain (MonoBehaviour runner, CompoundTask rootTask, Dictionary<string, WorldSensor> worldState)
		{
			_rootTask = rootTask;
			_plannner = new HTNPlanner(_rootTask);
			_worldState = worldState;
			_runner = new HTNRunner(runner, worldState);
		}

		private void Execute ()
		{
			Planning();
			if (_runTaskList == null)
			{
#if UNITY_EDITOR
				Debug.LogError($"[HTNDomain.Execute] Planning Fail");
#endif
				return;
			}
			Running();
		}

		private void Planning ()
		{
			if (_htnState == HTNState.Planning || _htnState == HTNState.Running)
			{
				return;
			}

			_htnState = HTNState.Planning;

			_runTaskList = _plannner.Plan(_worldState);
		}
		
		private void Running ()
		{
			if (_htnState == HTNState.Running || _runner.Status == TaskStatus.Running)
			{
				return;
			}
			// 重新规划
			if (_runner.Status == TaskStatus.Failure || _runner.Status == TaskStatus.Success)
			{
				_runTaskList = null;
				RePlan();
			}

			_htnState = HTNState.Running;
			_runner.Execute(_runTaskList);
		}

		private void RePlan ()
		{
			_htnState = HTNState.None;
		}
		
		

	}
}

