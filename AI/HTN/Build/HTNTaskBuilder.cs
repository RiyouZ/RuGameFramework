using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RuAI.HTN
{
	public static class HTNTaskBuilder
	{
		private static CompoundTask _root;

		public static CompoundBuilder<T> Compound<T> (string name, Agent agent) where T : CompoundTask
		{
			var compoundBuilder = new CompoundBuilder<T>(null, name, agent);
			_root = compoundBuilder.Task;
			return compoundBuilder;
		}

		public static CompoundBuilder<CompoundTask> Compound (string name, Agent agent)
		{
			return Compound<CompoundTask>(name, agent);
		}
	}
}

