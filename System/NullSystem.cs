using RuGameFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.System
{
	public class NullSystem : IRuSystem
	{
		public SystemType SystemType => SystemType.None;

		public void Init ()
		{
			Debug.LogError("Null System");
		}

		public void Dispose (){}
	}
}

