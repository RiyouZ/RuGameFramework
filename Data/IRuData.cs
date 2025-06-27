using RuGameFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Data
{
	public interface IRuData
	{
		public long ID
		{
			get; set;
		}

		// ��ʼ�� ��һ������ʱִ��

		public void Init ();

		public void Destory ();
		public IRuData Clone ();
	}
}