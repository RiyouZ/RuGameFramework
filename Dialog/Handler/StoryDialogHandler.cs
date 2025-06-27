using RuDialog.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog
{
	public class StoryDialogHandler
	{
		private DialogTree _currentStoryTree;

		private bool _isInteracting;
		private MonoBehaviour _runner;
		private Coroutine _asyncHandle;

		public Action<BaseDialogNode> OnStoryStart
		{
			get; set;
		}

		public Action<BaseDialogNode> OnStoryEnd
		{
			get; set;
		}

		public void InterruptStory()
		{
			if (!_isInteracting)
			{
				return;
			}

			_isInteracting = false;
		}

		public void StartStory (MonoBehaviour runner, DialogContext ctx, DialogTree curTree)
		{
			// 当前已经存在对话
			if (_isInteracting || _asyncHandle != null)
			{
				return;
			}

			// 发起者
			_runner = runner;
			_currentStoryTree = curTree;
			_asyncHandle = _runner.StartCoroutine(AsyncStartStory(ctx));
		}

		private IEnumerator AsyncStartStory (DialogContext ctx)
		{
			_isInteracting = true;

			var globalCtx = ctx.GetContext(DialogContextType.Global) as GlobalDialogContext;
			var curNode = _currentStoryTree.CurrentNode;
			BaseDialogNode preNode = null;

			OnStoryStart?.Invoke(curNode);
			while (curNode != null || !_isInteracting)
			{
				// 交互式对话 谁执行 谁就是actor
				yield return _currentStoryTree.Execute(ctx);
				_currentStoryTree.NextStep();
				yield return null;
				preNode = curNode;
				curNode = _currentStoryTree.CurrentNode;
				if (curNode != null && curNode.GameObject == null)
				{
#if UNITY_EDITOR
					Debug.LogError($"[Dialog.DispatchDialog] {curNode.name} Is Destroy");
#endif
					break;
				}
			}
			_isInteracting = false;
			OnStoryStart?.Invoke(preNode);

			if (_asyncHandle != null)
			{
				_runner.StopCoroutine(_asyncHandle);
				_runner = null;
				_asyncHandle = null;
			}
			
		}

	}

}

