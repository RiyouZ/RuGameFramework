using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace RuAI.HTN
{
	public class HTNRunner 
	{
		// ����ʱ���ݵ���Ϣ
		public class RunnerContext
		{
			public TaskStatus status;
			// �����״̬ �������������ӽ� �����ӽ�״̬��Agent����
			public Dictionary<string, WorldSensor> worldState;
		}

		private RunnerContext _ctx;
		
		public TaskStatus Status => _ctx.status;
		private List<PrimitiveTask> _currentTaskList;
		public MonoBehaviour _runner;
		private Coroutine _asyncRunHandle = null;
		public HTNRunner (MonoBehaviour runner, Dictionary<string, WorldSensor> worldState)
		{
			_ctx = new RunnerContext ();
			_runner = runner;
			_ctx.worldState = worldState;
		}

		public Action OnTaskRunStart
		{
			set; private get;
		}

		public Action OnTaskRunEnd
		{
			set; private get;
		}

		public void Execute (List<PrimitiveTask> taskList)
		{
			if (_currentTaskList == null)
			{
				_currentTaskList = taskList;
			}

			if (_asyncRunHandle != null)
			{
				return;
			}

			_asyncRunHandle = _runner.StartCoroutine(AsyncRunning(_currentTaskList));
		}

		private IEnumerator AsyncRunning (List<PrimitiveTask> taskList)
		{
			// ִ��
			_ctx.status = TaskStatus.Running;
			foreach (var task in taskList)
			{
				// ������ִ��ʧ��
				if (!task.Condition(_ctx.worldState))
				{
					_ctx.status = TaskStatus.Failure;
					_runner.StopCoroutine(_asyncRunHandle);
					_asyncRunHandle = null;
					yield break;
				}

				yield return task.Run(_ctx);
				if (_ctx.status == TaskStatus.Failure)
				{
					_runner.StopCoroutine(_asyncRunHandle);
					_asyncRunHandle = null;
					yield break;
				}
			}

			// ȫ��ִ�гɹ�
			_ctx.status = TaskStatus.Success;
			_runner.StopCoroutine(_asyncRunHandle);
			_asyncRunHandle = null;
		}
	}

}
