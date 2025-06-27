using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuDialog.Node
{
	public interface IDialogNode<T>
	{
		public GameObject GameObject
		{
			get;
			set;
		}

		public T Parent
		{
			get;
		}
		public List<T> Childs
		{
			get;
		}

		public IEnumerator Execute (DialogContext ctx);
		public T GetNextNode ();

		public T Clone (T parent);

	}

}
