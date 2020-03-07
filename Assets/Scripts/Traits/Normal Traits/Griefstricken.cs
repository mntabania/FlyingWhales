using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Griefstricken : Status {
        public Character owner { get; private set; }

        public Griefstricken() {
            name = "Griefstricken";
            description = "This character is grieving and may refuse to eat.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            moodEffect = -12;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.25f;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //owner.AdjustMoodValue(-20, this);
            }
            base.OnAddTrait(sourcePOI);
        }
        #endregion

        //public GoapPlanJob TriggerGrieving() {
        //    owner.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);

        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.GRIEVING, owner, owner);
        //    owner.jobQueue.AddJobInQueue(job);
        //    return job;
        //}

        public bool TriggerGrieving() {
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Grieving, owner);
        }
    }
}