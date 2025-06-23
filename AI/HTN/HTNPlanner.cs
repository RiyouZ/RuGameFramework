using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RuAI.HTN
{
	public class HTNPlanner
	{
		private class BackTrackContext
		{
			public CompoundTask task;
			public Dictionary<string, WorldSensor> worldState;
			public int resultCount;
			public int taskCount;
			// �����������
			public int waitTaskCount;

			public BackTrackContext (CompoundTask task, Dictionary<string, WorldSensor> worldState, int resultCount, int taskCount, int waitCount)
			{
				this.task = task;
				this.worldState = worldState;
				this.resultCount = resultCount;
				this.taskCount = taskCount;
				this.waitTaskCount = waitCount;
			}
		}

		// ������
		private CompoundTask _rootTask;
		private Stack<PrimitiveTask> _taskResult;
		private Stack<IHTNTask> _taskStack;
		// ��������ջ
		private Stack<BackTrackContext> _backStack;

        public HTNPlanner(CompoundTask rootTask)
        {
			_rootTask = rootTask;
			_taskResult = new Stack<PrimitiveTask>();
			_backStack = new Stack<BackTrackContext>();
			_taskStack = new Stack<IHTNTask>();
        }

        public List<PrimitiveTask> Plan (Dictionary<string, WorldSensor> worldState, CompoundTask compoundTask = null)
		{
			if (compoundTask == null)
			{
				compoundTask = _rootTask;
			}

			var copyWorldState = HTNSensors.CloneWorldState(worldState);

			_taskResult.Clear();
			_taskStack.Clear();
			_taskStack.Push(compoundTask);

			while (_taskStack.Count > 0)
			{
				var task = _taskStack.Pop();
				if (task is CompoundTask cpdTask)
				{
					// ȫ���޷�ִ��
					if (!cpdTask.Condition(copyWorldState))
					{
						// ����ʧ��
						if (!BackTrack(ref copyWorldState))
						{
							return null;
						}
						continue;
					}

					// ����״̬
					BackTrackContext backCtx = new BackTrackContext(cpdTask, HTNSensors.CloneWorldState(copyWorldState), _taskResult.Count, _taskStack.Count, cpdTask.CurrentMethod.subTask.Count);
					_backStack.Push(backCtx);

					var method = cpdTask.CurrentMethod;
					for (int i = method.subTask.Count - 1; i >= 0; i--)
					{
						_taskStack.Push(method.subTask[i]);
					}
				}
				else if (task is PrimitiveTask primitiveTask)
				{
					// ����ʧ��
					if (!primitiveTask.Condition(copyWorldState))
					{
						// ����ʧ��
						if (!BackTrack(ref copyWorldState))
						{
							return null;
						}
						continue;
					}

					// ���ȫ������ɹ� ���˵�����ջ��ջ��
					if (_backStack.Count > 0)
					{
						var ctx = _backStack.Peek();
						
						// ����������
						ctx.waitTaskCount--;

						// // ��Compound�Ѿ��ɹ���� ��һ���Ĵ����������� - 1
						while(_backStack.Count > 0 && _backStack.Peek().waitTaskCount <= 0)
						{
							_backStack.Pop();

							if (_backStack.Count <= 0)
							{
								break;
							}

							ctx = _backStack.Peek();
							ctx.waitTaskCount--;
						}
		
					}

					primitiveTask.Plan(copyWorldState);

					_taskResult.Push(primitiveTask);
				}
			}
			return _taskResult.Reverse().ToList();
		}

		private bool BackTrack (ref Dictionary<string, WorldSensor> worldState)
		{
			// û�п��Ի��˵�Task �滮ʧ��
			if (_backStack.Count <= 0)
			{
				return false;
			}

			// ���˵�ʱ�� ������Ѿ������������
			while (_backStack.Count > 0 && _backStack.Peek().waitTaskCount <= 0)
			{
				_backStack.Pop();
			}

			// ȫ�������û�п��Ի�����
			if (_backStack.Count <= 0)
			{
				return false;
			}

			// ����
			var context = _backStack.Pop();
			// ����״̬ ��Ҫ�޸��ⲿ����
			worldState = context.worldState;

			// ��������
			while (_taskResult.Count > context.resultCount)
			{
				_taskResult.Pop();
			}

			while (_taskStack.Count > context.taskCount)
			{
				_taskStack.Pop();
			}

			// ѡ����ݵ���һ�������ѡ��
			context.task.CurrentMethodIndex++;
			// ���·ֽ�
			_taskStack.Push(context.task);
			return true;
		}

	}

}
