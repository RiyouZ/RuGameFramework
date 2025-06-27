using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog.Context
{
	public enum DialogContextType
	{
		Global,		// 全局
		Local,		// 本局部树
		Node		// 节点
	}

	public interface IDialogContext
	{

		public void Clear ();
	}

}
