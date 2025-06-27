using RuDialog.Selector;
using RuGameFramework.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuDialog
{
	public static class DialogManager
	{
		private static InteractDialogHandler _interactHandler = new InteractDialogHandler();

		private static IDialogTreeSelector _selector;

		public static void Setting (IDialogTreeSelector selector)
		{
			_selector = selector;
		}

		// 选择Tree
		public static IEnumerator AsyncSelectTree (ISpeakableActor actor)
		{
			yield return _selector.Resolve(actor.Flag.GetHash(), tree => actor.Tree = tree.Clone());
		}

		public static IEnumerator AsyncSelectTree (List<ISpeakableActor> actorList)
		{
			foreach (ISpeakableActor actor in actorList)
			{
				yield return AsyncSelectTree(actor);
				yield return null;
			}
		}

		public static IEnumerator AsyncSelectTreeByName (ISpeakableActor actor)
		{
			yield return _selector.Resolve(actor.ActorName, actor.Flag.GetHash(), tree => actor.Tree = tree.Clone());
		}

		// 调用对话
		public static void StartDialog (MonoBehaviour runner, DialogContext ctx, Action<ISpeakableActor> onStart = null, Action<ISpeakableActor> onEnd = null)
		{
			_interactHandler.OnDialogStart = onStart;
			_interactHandler.OnDialogEnd = onEnd;
			_interactHandler.StartDialog(runner, ctx);
		}

		public static void StartDialog (MonoBehaviour runner, DialogContext ctx, List<ISpeakableActor> dialogActorList, Action<ISpeakableActor> onStart = null, Action<ISpeakableActor> onEnd = null)
		{
			_interactHandler.OnDialogStart = onStart;
			_interactHandler.OnDialogEnd = onEnd;
			_interactHandler.StartDialog(runner, ctx, dialogActorList);
		}


	}

}
