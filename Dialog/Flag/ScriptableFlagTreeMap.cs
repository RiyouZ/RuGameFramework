using RuGameFramework.Serialize;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RuDialog.Flag
{
	[CreateAssetMenu(fileName = "FlagTreeMap", menuName = "Dialog / FlagTreeMap")]
	public class ScriptableFlagTreeMap : SerializedScriptableObject
	{
		[DictionaryDrawerSettings(KeyLabel = "Flag", ValueLabel = "TreePath")]
		public Dictionary<string, string> treeMap = new Dictionary<string, string>();


		[Button]
		public void Save ()
		{
			AssetDatabase.SaveAssetIfDirty(this);
			AssetDatabase.Refresh();
		}
	}

}
