using Game.GamePlay.Scene.Data;
using RuGameFramework.Args;
using RuGameFramework.Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuGameFramework.Core
{
	public interface IRuSystem
	{
		public  SystemType SystemType {get; }
		public void Init ();
		public void Dispose ();
	}

	
}
