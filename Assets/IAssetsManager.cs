using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Assets
{
	public interface IAssetsManager
	{
		public void LoadAssets<T> (string path, Action<T> onComplate, Action<T> onFail = null) where T : UnityEngine.Object;

	}

}
