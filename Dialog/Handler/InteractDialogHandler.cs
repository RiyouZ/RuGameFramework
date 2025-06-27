using RuDialog.Context;
using RuDialog.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog
{
	public class InteractDialogHandler
	{
		private List<ISpeakableActor> _currentDialogActorList;

		private bool _isInteracting;
		private MonoBehaviour _runner;
		private Coroutine _asyncHandle;

		public Action<ISpeakableActor> OnDialogStart
		{
			get; set;
		}

		public Action<ISpeakableActor> OnDialogEnd
		{
			get; set;
		}

		public void InterruptDialog ()
		{
			if (!_isInteracting)
			{
				return;
			}

			_isInteracting = false;
		}

		// ��Context�����Ի�˳��
		public void StartDialog (MonoBehaviour runner, DialogContext ctx)
		{
			// ��ǰ�Ѿ����ڶԻ�
			if (_isInteracting || _asyncHandle != null)
			{
				return;
			}

			// ˵��������
			_runner = runner;
			_asyncHandle = _runner.StartCoroutine(AsyncDispatchDialog(ctx));
		}

		private IEnumerator AsyncDispatchDialog (DialogContext ctx)
		{
			_isInteracting = true;
			
			var globalCtx = ctx.GetContext(DialogContextType.Global) as GlobalDialogContext;
			var curActor = globalCtx.curActor;
			ISpeakableActor preActor = null;

			OnDialogStart?.Invoke(curActor);
			while (globalCtx.curActor != null || !_isInteracting)
			{
				// ����ʽ�Ի� ˭ִ�� ˭����actor
				curActor.Tree.currentNode.GameObject = curActor.gameObject;
				yield return curActor.Tree.Execute(ctx);
				curActor.Tree.NextStep();

				yield return null;
				preActor = curActor;
				curActor = globalCtx.nextActor;
				if (curActor.gameObject == null)
				{
#if UNITY_EDITOR
					Debug.LogError($"[Dialog.DispatchDialog] {curActor.name} Is Destroy");

#endif
					break;
				}
			}
			_isInteracting = false;
			OnDialogEnd?.Invoke(preActor);

			_runner.StopCoroutine(_asyncHandle);
			_runner = null;
			_asyncHandle = null;
		}

		// �����б�����Ի�˳��
		public void StartDialog (MonoBehaviour runner, DialogContext ctx, List<ISpeakableActor> dialogActorList)
		{
			// ��ǰ�Ѿ����ڶԻ�
			if (_isInteracting || _asyncHandle != null)
			{
				return;
			}

			if (dialogActorList.Count == 0)
			{
				return;
			}

			_currentDialogActorList = dialogActorList;
			_runner = runner;
			_asyncHandle = _runner.StartCoroutine(AsyncDispatchDialogActors(ctx));
		}

		private IEnumerator AsyncDispatchDialogActors (DialogContext ctx)
		{
			_isInteracting = true;
			int speakIndex = 0;
			int preIndex = 0;

			OnDialogStart?.Invoke(_currentDialogActorList[speakIndex]);

			long bitSpeakEnd = (1 << _currentDialogActorList.Count) - 1;
			while (_isInteracting || bitSpeakEnd == 0)
			{
				// �Ѿ���������
				if ((bitSpeakEnd >> speakIndex & 1L)  == 0)
				{
					speakIndex = ( speakIndex + 1 ) % _currentDialogActorList.Count;
					yield return null;
					continue;
				}

				// ��ǰ�������
				if (_currentDialogActorList[speakIndex].Tree.currentNode == null)
				{
					bitSpeakEnd &= ~( 1L << speakIndex );
				}
				else
				{
					// ����ʽ�Ի� ˭ִ�� ˭����actor
					var currentActor = _currentDialogActorList[speakIndex];
					currentActor.Tree.currentNode.GameObject = currentActor.ActorObject;

					yield return currentActor.Tree.Execute(ctx);
					currentActor.Tree.NextStep();

					preIndex = speakIndex;
				}

				
				speakIndex = (speakIndex + 1) % _currentDialogActorList.Count;
				yield return null;
			}

			OnDialogEnd?.Invoke(_currentDialogActorList[preIndex]);

			_runner.StopCoroutine(_asyncHandle);
			_runner = null;
			_asyncHandle = null;
		}
	}

}
