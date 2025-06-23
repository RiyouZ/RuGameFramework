using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI.HTN
{
	public class PrimitiveTask : BaseTask
	{
		protected Action<Dictionary<string, WorldSensor>> _effect;
		protected Action<HTNRunner.RunnerContext, PrimitiveTask> _run;
		public Action<HTNRunner.RunnerContext, PrimitiveTask> Do
		{
			set => _run = value;
		}

		public Action<Dictionary<string, WorldSensor>> Effect
		{
			set => _effect = value;
		}

		public static PrimitiveTask Create (string name, Agent agent, Func<Dictionary<string, WorldSensor>, bool> condition, Action<Dictionary<string, WorldSensor>> effect)
		{
			var task = Create<PrimitiveTask>(name, agent);
			task.AddCondition(condition);
			task._effect = effect;
			return task;
		}

		public static T Create<T> (string name, Agent agent) where T : PrimitiveTask
		{
			var task = CreateInstance<T>();
			task.taskName = name;
			task.CharacterAgent = agent;
			return task;
		}

		public static PrimitiveTask Create (string name, Agent agent)
		{
			var task = CreateInstance<PrimitiveTask>();
			task.taskName = name;
			return Create<PrimitiveTask>(name, agent);
		}

		public override void Initialize (IHTNTask parent)
		{
			this.parent = parent as BaseTask;
		}

		// 规划时 
		public virtual void Plan (Dictionary<string, WorldSensor> worldState)
		{
			ApplyEffect(worldState);
		}

		// 运行时
		public IEnumerator Run (HTNRunner.RunnerContext ctx)
		{
			yield return OnRunStart(ctx);
			yield return OnRun(ctx);
			yield return OnRunEnd(ctx);
			// 影响的是世界状态
			ApplyEffect(ctx.worldState);
		}

		// Effect 影响世界状态

		protected virtual IEnumerator OnRunStart (HTNRunner.RunnerContext ctx)
		{
			yield return null;
		}

		protected virtual IEnumerator OnRun (HTNRunner.RunnerContext ctx)
		{
			_run?.Invoke(ctx, this);
			yield return null;
		}

		protected virtual IEnumerator OnRunEnd (HTNRunner.RunnerContext ctx)
		{
			yield return null;
		}

		protected virtual void ApplyEffect (Dictionary<string, WorldSensor> worldState = null)
		{
			_effect?.Invoke(worldState);
		}
	}
}

