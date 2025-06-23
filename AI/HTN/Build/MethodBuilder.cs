using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI.HTN
{
	public interface IMethodBuilderChild
	{
		public IMethodBuilder Parent
		{
			get;
		}
		 
		public IHTNTask MethodChildTask
		{
			get;
		}
	}

	public interface IMethodBuilder
	{
		public Method MethodTask
		{
			get;
		}

		public PrimitiveBuilder<TType> Primitive<TType> (string name) where TType : PrimitiveTask;

		public PrimitiveBuilder<PrimitiveTask> Primitive (string name);

		public CompoundBuilder<TType> Compound<TType> (string name) where TType : CompoundTask;

		public CompoundBuilder<CompoundTask> Compound (string name);

		public ICompoundBuilder End ();
	}

	public class MethodBuilder<T> : IMethodBuilder where T : Method
	{
		protected ICompoundBuilder _parent;
		public Method MethodTask
		{
			get; protected set;
		}

		protected IMethodBuilderChild _child;

		public MethodBuilder (ICompoundBuilder parent, string name, Agent agent)
		{
			MethodTask = Method.Create<T>(name, agent);

			if (parent != null)
			{
				_parent = parent;
			}
		}

		public PrimitiveBuilder<TType> Primitive<TType> (string name) where TType : PrimitiveTask
		{
			_child = new PrimitiveBuilder<TType>(this, name, MethodTask.CharacterAgent);
			MethodTask.AddSubTask(_child.MethodChildTask);
			return _child as PrimitiveBuilder<TType>;
		}

		public PrimitiveBuilder<PrimitiveTask> Primitive (string name)
		{
			return Primitive<PrimitiveTask>(name);
		}

		public CompoundBuilder<TType> Compound<TType> (string name) where TType : CompoundTask
		{
			_child = new CompoundBuilder<TType>(this, name, MethodTask.CharacterAgent);
			MethodTask.AddSubTask(_child.MethodChildTask);
			return _child as CompoundBuilder<TType>;
		}

		public CompoundBuilder<CompoundTask> Compound (string name)
		{
			return Compound<CompoundTask>(name);
		}

		public MethodBuilder<T> Condition (Func<Dictionary<string, WorldSensor>, bool> func)
		{
			MethodTask.AddCondition(func);
			return this;
		}

		public ICompoundBuilder End ()
		{
			return _parent;
		}


	}
}
