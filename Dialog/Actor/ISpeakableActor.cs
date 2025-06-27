using RuDialog.Flag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog
{
	public interface ISpeakableActor
	{
		public GameObject ActorObject
		{
			get;
		}

		public string ActorName
		{
			get;
		}


		public DialogTree Tree
		{
			get; set;
		}

		public ScriptableFlag Flag
		{
			get;
		}

		public IEnumerator Speak (DialogContext ctx, string content);

	}

}
