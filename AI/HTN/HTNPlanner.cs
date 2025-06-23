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
			// 待处理的任务
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

		// 根任务
		private CompoundTask _rootTask;
		private Stack<PrimitiveTask> _taskResult;
		private Stack<IHTNTask> _taskStack;
		// 操作回退栈
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
					// 全部无法执行
					if (!cpdTask.Condition(copyWorldState))
					{
						// 回退失败
						if (!BackTrack(ref copyWorldState))
						{
							return null;
						}
						continue;
					}

					// 保存状态
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
					// 条件失败
					if (!primitiveTask.Condition(copyWorldState))
					{
						// 回退失败
						if (!BackTrack(ref copyWorldState))
						{
							return null;
						}
						continue;
					}

					// 如果全部处理成功 则退掉回退栈的栈顶
					if (_backStack.Count > 0)
					{
						var ctx = _backStack.Peek();
						
						// 待处理任务
						ctx.waitTaskCount--;

						// // 该Compound已经成功完成 上一个的待处理任务数 - 1
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
			// 没有可以回退的Task 规划失败
			if (_backStack.Count <= 0)
			{
				return false;
			}

			// 回退的时候 清除掉已经处理过的任务
			while (_backStack.Count > 0 && _backStack.Peek().waitTaskCount <= 0)
			{
				_backStack.Pop();
			}

			// 全部清空则没有可以回退项
			if (_backStack.Count <= 0)
			{
				return false;
			}

			// 回溯
			var context = _backStack.Pop();
			// 回溯状态 需要修改外部引用
			worldState = context.worldState;

			// 回溯任务
			while (_taskResult.Count > context.resultCount)
			{
				_taskResult.Pop();
			}

			while (_taskStack.Count > context.taskCount)
			{
				_taskStack.Pop();
			}

			// 选择回溯到上一个任务的选择
			context.task.CurrentMethodIndex++;
			// 重新分解
			_taskStack.Push(context.task);
			return true;
		}

	}

}
