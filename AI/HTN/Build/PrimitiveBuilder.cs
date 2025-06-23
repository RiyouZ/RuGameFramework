using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace RuAI.HTN
{

	public interface IPrimitiveBuilder : IMethodBuilderChild
	{
		public PrimitiveTask Task 
		{
			get;
		}
	}

	public class PrimitiveBuilder<T> : IPrimitiveBuilder where T : PrimitiveTask
	{
		public PrimitiveTask Task
		{
			get; protected set;
		}

		protected IMethodBuilder _parent;
		public IMethodBuilder Parent => _parent;

		IHTNTask IMethodBuilderChild.MethodChildTask => Task;

		public PrimitiveBuilder (IMethodBuilder parent, string name, Agent agent)
		{
			Task = PrimitiveTask.Create<T>(name, agent);

			if (parent != null)
			{
				_parent = parent;
			}
		}

		public PrimitiveBuilder<T> If (Func<Dictionary<string, WorldSensor>, bool> func)
		{
			Task.AddCondition(func);
			return this;
		}

		public PrimitiveBuilder<T> Run (Action<HTNRunner.RunnerContext, PrimitiveTask> func)
		{
			Task.Do = func;
			return this;
		}

		public PrimitiveBuilder<T> Effect (Action<Dictionary<string, WorldSensor>> func)
		{
			Task.Effect = func;
			return this;
		}

		public IMethodBuilder End ()
		{
			return _parent;
		}

	}

}
