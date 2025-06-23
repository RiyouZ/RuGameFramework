using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace RuAI.HTN
{
	public interface ICompoundBuilder : IMethodBuilderChild
	{
		public CompoundTask Task
		{
			get;
		}

		public MethodBuilder<TType> Method<TType> (string name) where TType : Method;

		public MethodBuilder<Method> Method (string name);

		public CompoundBuilder<TType> EndMethod<TType> () where TType : CompoundTask;

		public IMethodBuilder End ();

		public CompoundTask EndCompoundBuild ();
	}

	public class CompoundBuilder<T> : ICompoundBuilder where T : CompoundTask
	{
		protected IMethodBuilder _parent;

		public CompoundTask Task
		{
			get; private set;
		}

		IMethodBuilder IMethodBuilderChild.Parent => _parent;
		BaseTask IMethodBuilderChild.MethodChildTask => Task;

		protected IMethodBuilder _currentMethod;

		public CompoundBuilder (IMethodBuilder parent, string name, Agent agent) 
		{
			Task = CompoundTask.Create<T>(name, agent);

			if (parent != null)
			{
				_parent = parent;
			}
		}

		public MethodBuilder<TType> Method<TType> (string name) where TType : Method
		{
			_currentMethod = new MethodBuilder<TType>(this, name, Task.CharacterAgent);
			Task.AddMethod(_currentMethod.MethodTask);
			return _currentMethod as MethodBuilder<TType>;
		}

		public MethodBuilder<Method> Method (string name)
		{
			return Method<Method>(name);
		}

		public CompoundBuilder<TType> EndMethod<TType> () where TType : CompoundTask
		{
			return this as CompoundBuilder<TType>;
		}

		public IMethodBuilder End ()
		{
			return _parent;
		}

		public CompoundTask EndCompoundBuild ()
		{
			return Task;
		}



	}
}
