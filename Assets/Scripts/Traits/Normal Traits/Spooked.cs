using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Spooked : Status {
        //public List<Character> terrifyingCharacters { get; private set; }
        public Character owner { get; private set; }

        public Spooked() {
            name = "Spooked";
            description = "This character is too scared and may refuse to sleep.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
            //terrifyingCharacters = new List<Character>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //owner.AdjustMoodValue(-10, this);
            }
            base.OnAddTrait(sourcePOI);
        }
        //public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        //    if (sourcePOI is Character) {
        //        Character character = sourcePOI as Character;
        //    }
        //    base.OnRemoveTrait(sourcePOI, removedBy);
        //}
        //public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        //    base.OnSeePOI(targetPOI, character);
        //    if (targetPOI is Character) {
        //        Character targetCharacter = targetPOI as Character;
        //        if (character.combatComponent.AddAvoidInRange(targetCharacter)) {
        //            AddTerrifyingCharacter(targetCharacter);
        //        }
        //    }
        //}
        #endregion

        public bool TriggerFeelingSpooked() {
            //owner.needsComponent.SetHasCancelledSleepSchedule(false);
            //owner.needsComponent.ResetSleepTicks();
            //owner.jobQueue.CancelAllJobs(JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED);

            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_SPOOKED, owner, owner);
            //owner.jobQueue.AddJobInQueue(job);
            //return job;
            return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Spooked, owner);
        }
    }

    //public class SaveDataSpooked : SaveDataTrait {
    //    //public List<int> terrifyingCharacterIDs;

    //    public override void Save(Trait trait) {
    //        base.Save(trait);
    //        Spooked derivedTrait = trait as Spooked;
    //        //for (int i = 0; i < derivedTrait.terrifyingCharacters.Count; i++) {
    //        //    terrifyingCharacterIDs.Add(derivedTrait.terrifyingCharacters[i].id);
    //        //}
    //    }

    //    public override Trait Load(ref Character responsibleCharacter) {
    //        Trait trait = base.Load(ref responsibleCharacter);
    //        Spooked derivedTrait = trait as Spooked;
    //        //for (int i = 0; i < terrifyingCharacterIDs.Count; i++) {
    //        //    derivedTrait.AddTerrifyingCharacter(CharacterManager.Instance.GetCharacterByID(terrifyingCharacterIDs[i]));
    //        //}
    //        return trait;
    //    }
    //}

}
