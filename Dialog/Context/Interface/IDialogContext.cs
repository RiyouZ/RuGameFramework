using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog.Context
{
	public enum DialogContextType
	{
		Global,		// ȫ��
		Local,		// ���ֲ���
		Node		// �ڵ�
	}

	public interface IDialogContext
	{

		public void Clear ();
	}

}
