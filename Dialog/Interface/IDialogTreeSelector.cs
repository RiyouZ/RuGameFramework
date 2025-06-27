using RuDialog;
using RuDialog.Selector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog.Selector
{
	public interface IDialogTreeSelector
	{
		public IEnumerator Resolve (object key, Action<DialogTree> currentTree);
		public IEnumerator Resolve (string name, object key, Action<DialogTree> currentTree);
	}

	public interface IDialogTreeSelector<T> : IDialogTreeSelector
	{
		public IEnumerator Resolve (T key, Action<DialogTree> currentTree);

		public IEnumerator Resolve (string name, T key, Action<DialogTree> currentTree);
	}

}


