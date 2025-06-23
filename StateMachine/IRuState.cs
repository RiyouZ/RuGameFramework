using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.StateMachine
{
	public interface IRuState<T>
	{
		public T Index
		{
			set; get;
		}

		public List<StateTransition<T>> StateTransitions
		{
			set; get;
		}

		// �������
		public IRuState<T> AddTransition (IRuState<T> toState, Func<bool> cond);

		public IRuState<T> AddTransition (T toStateIndex, Func<bool> cond);

		// ����״̬
		public void Enter ();

		// ����״̬
		public void Update (float deltaTime);

		// �˳�״̬
		public void Exit ();
	}
}
