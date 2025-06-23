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
	public class HTNDomain : IAIDriver
	{
		public HTNState DomainState => _htnState;

		private HTNState _htnState = HTNState.None;

		private CompoundTask _rootTask;

		private List<PrimitiveTask> _runTaskList;
		private HTNPlanner _plannner;
		private HTNRunner _runner;

		private Dictionary<string, WorldSensor> _worldState;

		private bool _isExecuting = false;
		private Coroutine _asyncHandle;

		public HTNDomain (MonoBehaviour runner, CompoundTask rootTask, Dictionary<string, WorldSensor> worldState)
		{
			_rootTask = rootTask;
			_rootTask.Initialize(null);
			_plannner = new HTNPlanner(_rootTask);
			_worldState = worldState;
			_runner = new HTNRunner(runner, worldState);
		}

		public HTNDomain (MonoBehaviour runner, CompoundTask rootTask, Agent agent, Dictionary<string, WorldSensor> worldState)
		{
			_rootTask = rootTask;
			_rootTask.InitializeAgent(agent);
			_plannner = new HTNPlanner(_rootTask);
			_worldState = worldState;
			_runner = new HTNRunner(runner, worldState);
		}

		private void Execute ()
		{
			if (!_isExecuting)
			{
				_isExecuting = true;
			}

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
			// 重新规划
			if (_runner.Status == TaskStatus.Failure || _runner.Status == TaskStatus.Success)
			{
				_runTaskList = null;
				RePlan();
				return;
			}

			if (_htnState == HTNState.Running || _runner.Status == TaskStatus.Running)
			{
				return;
			}

			_htnState = HTNState.Running;
			_runner.Execute(_runTaskList);
		}

		private IEnumerator AsyncExecute ()
		{
			_isExecuting = true;
			while (_isExecuting)
			{
				Execute();
				yield return null;
			}
		}

		private void RePlan ()
		{
			_htnState = HTNState.None;
			_runner.Reset();
		}

		public void Run ()
		{
			Execute();
		}

		public void Run (MonoBehaviour runner)
		{
			_asyncHandle = runner.StartCoroutine(AsyncExecute());
		}

		public void Stop (bool force = false)
		{
			_isExecuting = false;
			_runner.Stop(force);
			_htnState = HTNState.None;
		}

		public void Stop (MonoBehaviour runner, bool force = false)
		{
			if (_asyncHandle != null)
			{
				runner.StopCoroutine(_asyncHandle);
			}
			Stop(force);
		}

		public void Dispose ()
		{
			if (_isExecuting)
			{
				return;
			}

			_runTaskList.Clear();
			_runTaskList = null;
			_rootTask = null;
			_worldState = null;
			_plannner = null;
			_runner = null;
		}
	}
}

