using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Traits {
	public class ElementalMaster : Trait {
		
		public ElementalMaster() {
			name = "Elemental Master";
			description = "This character has mastered the elements.";
			type = TRAIT_TYPE.BUFF;
			effect = TRAIT_EFFECT.POSITIVE;
			ticksDuration = 0;
		}

		#region Overrides
		public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
			if (targetPOI is TornadoTileObject) {
				if (characterThatWillDoJob.stateComponent.currentState is CombatState 
				    && characterThatWillDoJob.combatComponent.avoidInRange.Count > 0) {
					if (characterThatWillDoJob.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
						characterThatWillDoJob.combatComponent.Flight(targetPOI, "saw a tornado");
					}
				} else {
					if(!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.SNUFF_TORNADO, targetPOI)) {
						GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SNUFF_TORNADO,
							INTERACTION_TYPE.SNUFF_TORNADO, targetPOI, characterThatWillDoJob);
						characterThatWillDoJob.jobQueue.AddJobInQueue(job);
					}	
				}
			}
			return base.OnSeePOI(targetPOI, characterThatWillDoJob);
		}
		#endregion
		
	}
}