using RuDialog.Node;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RuDialog
{
	public abstract class BaseDialogNode : ScriptableObject, IDialogNode<BaseDialogNode>
	{
		public string nodeName;

		[SerializeField] private GameObject _gameObject;
		public GameObject GameObject
		{
			get => _gameObject; 
			set => _gameObject = value;
		}

		private BaseDialogNode _parent;
		public BaseDialogNode Parent
		{
			get => _parent; 
			set => _parent = value;
		}

		[SerializeField] private List<BaseDialogNode> _childs;
		public List<BaseDialogNode> Childs
		{
			get => _childs;
			set => _childs = value;
		}

		public abstract IEnumerator Execute (DialogContext ctx);

		public abstract BaseDialogNode GetNextNode ();

		public virtual BaseDialogNode Clone (BaseDialogNode parent = null)
		{
			var cloneNode = ScriptableObject.Instantiate(this);
			cloneNode.Parent = parent;
			cloneNode.Childs = new List<BaseDialogNode> ();
			cloneNode.nodeName = this.nodeName;
			cloneNode.GameObject = this.GameObject;

			foreach (var child in Childs)
			{
				cloneNode.AddChild(child.Clone(cloneNode));
			}


			return cloneNode;
		}

		public virtual void Initialize (BaseDialogNode parent)
		{
			Parent = parent;
		}

		public virtual void OnAdd () { }
		public virtual void OnRemove () { }

		public virtual void AddChild<T> () where T : BaseDialogNode
		{
			var child = CreateInstance<T>();
			child.Initialize(this);
			child.OnAdd();
			Childs.Add(child);
		}

		public virtual void AddChild<T> (T childNode) where T : BaseDialogNode
		{
			childNode.Initialize(this);
			childNode.OnAdd();
			Childs.Add(childNode);
		}

		public virtual void RemoveChild<T> (T child) where T : BaseDialogNode
		{
			child.OnRemove();
			Childs.Remove(child);
		}
	}

}
