using RuGameFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Component
{
	public class NullCom : MonoBehaviour, IRuComponent
	{
		public ComponentType ComType => ComponentType.None;
		public GameObject BindObj => null;

		public void Dispose ()
		{
			
		}

		// TODO Log系统接入
		public void Init ()
		{
			
		}
	}
}

