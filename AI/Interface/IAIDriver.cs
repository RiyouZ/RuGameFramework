using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI
{
	public interface IAIDriver
	{
		public void Run ();
		public void Stop ();
		public void Dispose ();
	}

}
