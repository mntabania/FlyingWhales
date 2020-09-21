using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Bored : Status {

        public override bool isSingleton => true;

        public Bored() {
			name = "Bored";
			description = "Is lacking some sort of entertainment.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = 0;
			moodEffect = -8;
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Overrides
        public override void OnHourStarted(ITraitable traitable) {
			base.OnHourStarted(traitable);
            if(traitable is Character character) {
                if (!character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                    if (UnityEngine.Random.Range(0, 100) < 15) {
                        if (!character.partyComponent.isActiveMember) {
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), character, character);
                            character.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
		}
		#endregion
	}
}
