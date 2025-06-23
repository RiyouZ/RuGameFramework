using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI
{
	public interface IAIDriver
	{
		public void Run ();

		public void Run (MonoBehaviour runner);

		public void Stop (bool force);

		public void Stop (MonoBehaviour runner, bool force);

		public void Dispose ();
	}

}
