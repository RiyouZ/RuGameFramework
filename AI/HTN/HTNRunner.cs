using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace RuAI.HTN
{
	public class HTNRunner 
	{
		// 运行时传递的信息
		public class RunnerContext
		{
			public TaskStatus status;
			// 世界的状态 并非自身主观视角 主观视角状态由Agent管理
			public Dictionary<string, WorldSensor> worldState;
		}

		private RunnerContext _ctx;
		
		public TaskStatus Status => _ctx.status;
		public MonoBehaviour _runner;
		private Coroutine _asyncRunHandle = null;

		private bool _waitStop = false;

		public HTNRunner (MonoBehaviour runner, Dictionary<string, WorldSensor> worldState)
		{
			_ctx = new RunnerContext ();
			_runner = runner;
			_ctx.status = TaskStatus.None;
			_ctx.worldState = worldState;
		}

		public Action OnTaskRunStart
		{
			set; private get;
		}

		public Action<TaskStatus> OnTaskRunEnd
		{
			set; private get;
		}

		public void Execute (List<PrimitiveTask> taskList)
		{
			if (_asyncRunHandle != null)
			{
				return;
			}

			_asyncRunHandle = _runner.StartCoroutine(AsyncRunning(taskList));
		}

		public void Reset ()
		{
			_ctx.status = TaskStatus.None;
		}

		public void Stop (bool force = false)
		{
			if (_asyncRunHandle != null && force)
			{
				_runner.StopCoroutine(_asyncRunHandle);
				_asyncRunHandle = null;
				return;
			}

			_waitStop = true;
		}

		private IEnumerator AsyncRunning (List<PrimitiveTask> taskList)
		{
			if (taskList == null)
			{
				yield break;
			}

			// 执行
			_ctx.status = TaskStatus.Running;
			OnTaskRunStart?.Invoke();
			foreach (var task in taskList)
			{
				// 因条件执行失败
				if (!task.Condition(_ctx.worldState))
				{
					_ctx.status = TaskStatus.Failure;
					OnTaskRunEnd?.Invoke(_ctx.status);
					_runner.StopCoroutine(_asyncRunHandle);
					_asyncRunHandle = null;
					yield break;
				}

				yield return task.Run(_ctx);
				// 执行失败
				if (_ctx.status == TaskStatus.Failure)
				{
					OnTaskRunEnd?.Invoke(_ctx.status);
					_runner.StopCoroutine(_asyncRunHandle);
					_asyncRunHandle = null;
					yield break;
				}

				// 执行完停止
				if (_waitStop)
				{
					yield break;
				}
			}

			// 全部执行成功
			_ctx.status = TaskStatus.Success;
			OnTaskRunEnd?.Invoke(_ctx.status);
			_runner.StopCoroutine(_asyncRunHandle);
			_asyncRunHandle = null;
		}
	}

}
