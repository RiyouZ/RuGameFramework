using RuDialog.Flag;
using RuGameFramework.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuDialog.Selector
{
	public class FlagSelector : BaseTreeSelector<string>
	{

		private ScriptableFlagTreeMap _map;
		private IAsyncAssetLoadAdapter _loadAdapter;
		private bool _isMapLoadingComplete = false;

		public FlagSelector(string mapPath, IAsyncAssetLoadAdapter asyncAssetLoadAdapter)
        {
			_loadAdapter = asyncAssetLoadAdapter;
			_loadAdapter.AsyncLoadAsset<ScriptableFlagTreeMap>(mapPath, (map) => 
			{
				_map = map;
				_isMapLoadingComplete = true;
			});
        }

        public override IEnumerator Resolve (string key, Action<DialogTree> onResolveComplete)
		{
			// 等待加载完成
			while (!_isMapLoadingComplete)
				yield return null;

			if (!_map.treeMap.TryGetValue(key, out var treePath))
			{
				yield break;
			}

			yield return _loadAdapter.AsyncLoadAsset<DialogTree>(treePath, (tree) =>
		   {
			   onResolveComplete?.Invoke(tree);
		   });

			yield return null;
		}

		public override IEnumerator Resolve (string name, string key, Action<DialogTree> currentTree)
		{
			var newKey = string.Join(':', name, key);

			yield return Resolve(newKey, currentTree);
		}
	}

}
