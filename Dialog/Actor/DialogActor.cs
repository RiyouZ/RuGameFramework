using RuDialog.Flag;
using RuDialog.Node;
using RuDialog.Selector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuDialog
{
	public class DialogActor : MonoBehaviour, ISpeakableActor
	{
		public DialogTree Tree
		{
			get;
			set;
		}

		[SerializeField]
		protected ScriptableFlag _flag;
		public ScriptableFlag Flag => _flag;

		public string ActorName => this.name;

		public GameObject ActorObject => this.gameObject;

		void Start ()
		{

		}

		public IEnumerator Speak (DialogContext ctx, string content)
		{
			yield return OnSpeakStart(ctx);
			yield return OnSpeak(ctx, content);
			yield return OnSpeakEnd(ctx);
		}
		protected IEnumerator OnSpeakStart (DialogContext ctx)
		{
			yield return null;
		}

		protected IEnumerator OnSpeak (DialogContext ctx, string content)
		{
			Debug.Log($"[{ActorName}.Speak] {content}");
			yield return null;
		}

		protected IEnumerator OnSpeakEnd (DialogContext ctx)
		{
			yield return null;
		}

	}

}
