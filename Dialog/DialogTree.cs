using RuDialog.Node;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuDialog
{
	[CreateAssetMenu(fileName = "DialogTree", menuName = "Dialog / DialogTree")]
	public class DialogTree : SerializedScriptableObject
	{
		public string treeName;
		
		[ShowInInspector] private BaseDialogNode _currentNode;

		public BaseDialogNode CurrentNode
		{
			get
			{
				return _currentNode;
			}
		}
		[InlineEditor]
		public BaseDialogNode rootNode;

		public static DialogTree Create (BaseDialogNode root)
		{
			DialogTree tree = CreateInstance<DialogTree>();
			tree.rootNode = root;
			tree._currentNode = root;
			return tree;
		}

		public DialogTree Clone ()
		{
			var cloneRoot = rootNode.Clone();
			var cloneTree = CreateInstance<DialogTree>();
			cloneTree.rootNode = cloneRoot;
			cloneTree.name = treeName;
			cloneTree._currentNode = cloneRoot;
			return cloneTree;
		}

		private BaseDialogNode FindCurrentNodeForClone (BaseDialogNode clonedRoot, BaseDialogNode originalNode)
		{
			if (clonedRoot == null || originalNode == null)
				return null;

			if (clonedRoot.nodeName == originalNode.nodeName)
				return clonedRoot;

			foreach (var child in clonedRoot.Childs)
			{
				var result = FindCurrentNodeForClone(child, originalNode);
				if (result != null)
					return result;
			}

			return null;
		}

		public IEnumerator Execute (DialogContext ctx)
		{
			yield return _currentNode.Execute(ctx);
		}		

		public void NextStep ()
		{
			_currentNode = _currentNode.GetNextNode() as BaseDialogNode;
		}
	}

}
