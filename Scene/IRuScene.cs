using RuGameFramework.Args;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Core
{
	public interface IRuScene : IRuSystem
	{
		public void Init (ISceneArgs args);
		public void OnLoadScene ();
		public void Enter ();
		public void Update (float deltaTime);
		// Ë¢ÐÂ³¡¾°
		public void Reset ();
		public void Exit ();
	}
}
