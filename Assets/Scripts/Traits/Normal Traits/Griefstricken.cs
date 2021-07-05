using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Griefstricken : Status {
        public Character owner { get; private set; }

        public Griefstricken() {
            name = "Griefstricken";
            description = "Lost a loved one.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(72);
            moodEffect = -12;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.25f;
            hindersSocials = true;
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        #endregion
        
        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character character) {
                owner = character;
                if(responsibleCharacter != null) {
                    //Right now responsible character will only be null upon loading from saved game because the references here is not yet loaded
                    //So i added a null checker, since we also do not want to double the log addition of griefstricken
                    string description = "death";
                    if (!responsibleCharacter.isDead) {
                        description = "presumed death";
                    }
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", name, "gain", null, LogUtilities.Social_Life_Changes_Tags);
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(responsibleCharacter, responsibleCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, description, LOG_IDENTIFIER.STRING_1);
                    log.AddLogToDatabase(true);
                }
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