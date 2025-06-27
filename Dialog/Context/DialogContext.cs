using RuDialog.Context;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog
{
	public class DialogContext
	{
		private Dictionary<DialogContextType, IDialogContext> _dialogContextMap;

		public DialogContext ()
		{
			_dialogContextMap = new Dictionary<DialogContextType, IDialogContext> 
			{
				{DialogContextType.Global, new GlobalDialogContext()}
			};
		}

		public IDialogContext GetContext (DialogContextType type)
		{
			if (!_dialogContextMap.TryGetValue(type, out var ctx))
			{
				return null;
			}

			return ctx;
		}
	}
}
