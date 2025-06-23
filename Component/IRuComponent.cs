using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Core
{
	public interface IRuComponent
	{
		public ComponentType ComType {get; }

		public GameObject BindObj {get; }

		public void Init ();
		public void Dispose ();
	}


}
