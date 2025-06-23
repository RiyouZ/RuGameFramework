using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Core
{
	public interface IPoolItem
	{
		public void Init ();
		public void Reset ();
	}

	public interface IObjectPool<T>
	{
		// ��������
		public T Spawn ();
		// ���ն���
		public void Collection (T obj);
		// ���ٳ�
		public void Dispose ();
	}

}

