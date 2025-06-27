using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog.Selector
{
	public abstract class BaseTreeSelector<T> : IDialogTreeSelector<T>
	{
		public abstract IEnumerator Resolve (T key, Action <DialogTree> onResolveComplete);

		public abstract IEnumerator Resolve (string name, T key, Action<DialogTree> currentTree);

		public IEnumerator Resolve (object key, Action<DialogTree> currentTree)
		{
			if (key is T tKey)
			{
				yield return Resolve(tKey, currentTree);
			}
			else
			{
#if UNITY_EDITOR
				Debug.LogError("[TreeSelector.Resolve] Not Find Type Key");
#endif
				yield break;
			}
		
		}

		public IEnumerator Resolve (string name, object key, Action<DialogTree> currentTree)
		{
			if (key is T tKey)
			{
				yield return Resolve(name, tKey, currentTree);
			}
			else
			{
#if UNITY_EDITOR
				Debug.LogError("[TreeSelector.Resolve] Not Find Type Key");
#endif
				yield break;
			}
		}

		
	}

}
