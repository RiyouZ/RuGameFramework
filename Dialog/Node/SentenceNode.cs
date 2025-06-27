using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog.Node
{
	[CreateAssetMenu(fileName = "SentenceNode", menuName = "Dialog / Node / SentenceNode")]
	public class SentenceNode : BaseDialogNode
	{
		public string content;

		private ISpeakableActor _actor;
		public ISpeakableActor Actor
		{
			get
			{
				if (_actor == null)
				{
					_actor = GameObject.GetComponent<ISpeakableActor>();
				}
				return _actor;
			}
		}

		public override IEnumerator Execute (DialogContext ctx)
		{
			yield return Actor.Speak(ctx, content);
			yield return null;
		}

		public override BaseDialogNode GetNextNode ()
		{
			if (Childs == null || Childs.Count == 0)
			{
				return null;
			}
			return Childs[0];
		}
	}
}
