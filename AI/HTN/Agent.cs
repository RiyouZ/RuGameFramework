using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  RuAI.HTN
{
	// AI´úÀíÌå
	public class Agent : MonoBehaviour
	{
		// htn
		public IAIDriver runner;

		public Dictionary<string, WorldSensor> worldState;

		// ¿½±´×´Ì¬
		public Dictionary<string, WorldSensor> CopyState()
		{
			return null;
		}

		public void Start ()
		{
			WorldSensor sensor = new WorldSensor();
			sensor.Set(0);

			CompoundTask rTask = HTNTaskBuilder
				.Compound("Root", this)
						.Method("Method R1").Condition(_ => true)
								.Primitive("PrimitiveTask RA").If(_ => true)
										.Effect(_ => Debug.Log("PTask A "))
								.End()
								.Compound("SubCompoundTask A")
									.Method("Method A1").Condition(_ => true)
											.Primitive("PrimitiveTask B").If(_ => true)
														.Effect(_ => Debug.Log("PTask B"))
											.End()
											.Compound("SubCompoundTask B")
														.Method("Method B1").Condition(_ => true)
															.Primitive("PrimitiveTask BA").If(_ => true)
																.Effect(_ => Debug.Log("PTask BA"))
															.End()
														.End()
														.Method("Method B2").Condition(_ => true)
															.Primitive("PrimitiveTask BB").If(_ => true)
																.Effect(_ => Debug.Log("PTask BB"))
															.End()
														.End()
											.End()
											.Primitive("PrimitiveTask AC").If(_ => false)
														.Effect(_=> Debug.Log("PTask C"))
											.End()
									.End()
									.Method("Method A2").Condition(_ => true)
											.Primitive("PrimitiveTask AD").If(_ => true)
														.Effect(_ => Debug.Log("PTask D"))
											.End()
											.Primitive("PrimitiveTask AE").If(_ => true)
														.Effect(_ => Debug.Log("PTask E"))
											.End()
									.End()
								.End()
								.Primitive("PrimitiveTask RF").If(_ => true)
											.Effect(_=> Debug.Log("PTask F"))
								.End()
						.End()
				.EndCompoundBuild();


			var planner = new HTNPlanner(rTask);

			var state = new Dictionary<string, WorldSensor>();
			state.Add("count", sensor);

			var planResult = planner.Plan(state);
			foreach (var task in planResult)
			{
				Debug.Log($"[End Result] {task.taskName}");
			}

		}
	}
}

