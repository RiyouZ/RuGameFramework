using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Data
{
	public interface IRuData
	{
		// ��ʼ�� ��һ������ʱִ��
		public void Init ();

		public void Destory ();
		public IRuData Clone ();
	}
}