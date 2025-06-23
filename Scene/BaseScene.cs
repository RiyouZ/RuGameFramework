using System;
using RuGameFramework.Args;
using RuGameFramework.Core;
using RuGameFramework.System;
using UnityEngine;

namespace RuGameFramework.Scene
{
	public abstract class BaseScene : IRuScene
	{
		public SystemType SystemType => SystemType.BaseSceneSystem;

		protected GameObject _sceneObj;
		
		protected SystemContainer _sysContainer;

		protected bool _isActive = false;
		public abstract void Init ();
		public abstract void Init (ISceneArgs arg);
		public virtual void OnLoadScene ()
		{

		}

		public abstract void Enter ();

		public abstract void Update (float deltaTime);

		public abstract void Exit ();
		
		public abstract void ActiveScene (bool isActive);

		public abstract void Reset ();

		public virtual void Dispose ()
		{
			if (_sysContainer != null)
			{
				_sysContainer.Release();
				_sysContainer = null;
			}
		}
	}

}
