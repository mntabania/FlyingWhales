using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Heartbroken : Status {
        public Character owner { get; private set; }

        public Heartbroken() {
            name = "Heartbroken";
            description = "Experiencing a strong amount of emotional stress.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(48);
            moodEffect = -8;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.25f;
            hindersSocials = true;
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                owner = addTo as Character;
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //owner.AdjustMoodValue(-25, this);
            }
            base.OnAddTrait(sourcePOI);
        }
        #endregion

        //public GoapPlanJob TriggerBrokenhearted() {
        //    owner.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);

        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_BROKENHEARTED, owner, owner);
        //    owner.jobQueue.AddJobInQueue(job);
        //    return job;
        //}
        public bool TriggerBrokenhearted() {
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Brokenhearted, owner);
        }
    }
}

