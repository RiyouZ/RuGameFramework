using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Args
{
	// TODO ����������
	public interface IGameArgs 
	{
		public void Dispose ();
	}
	public interface ISceneArgs : IGameArgs {}
}


