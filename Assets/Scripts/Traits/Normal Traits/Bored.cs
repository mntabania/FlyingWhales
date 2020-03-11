using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Bored : Status {
		private Character owner;
		public Bored() {
			name = "Bored";
			description = "This character is feeling slightly down.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = 0;
			moodEffect = -8;
			//effects = new List<TraitEffect>();
		}

		#region Overrides
		public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			owner = addedTo as Character;
		}
		public override void OnHourStarted() {
			base.OnHourStarted();
			if (!owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
				if (UnityEngine.Random.Range(0, 100) < 15) {
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
					owner.jobQueue.AddJobInQueue(job);
				}
			}
		}
		#endregion
	}
}
