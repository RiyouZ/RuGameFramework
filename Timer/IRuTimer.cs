using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Core
{
	public interface IRuTimer
	{
		public void Update (float time);
		public void Release ();
		public void Reset ();
	}
}

