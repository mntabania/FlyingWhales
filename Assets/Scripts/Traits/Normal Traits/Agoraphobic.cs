using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Agoraphobic : Trait {

        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Agoraphobics avoid crowds.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        //protected override void OnChangeLevel() {
        //    base.OnChangeLevel();
        //if(level == 1) {
        //    daysDuration = 50;
        //} else if (level == 2) {
        //    daysDuration = 70;
        //} else if (level == 3) {
        //    daysDuration = 90;
        //}
        //}
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                ApplyAgoraphobicEffect(character);
                //if (character.marker.inVisionCharacters.Count >= 3) {
                //    ApplyAgoraphobicEffect(character, true);
                //}
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //Character targetCharacter = targetPOI as Character;
                if (characterThatWillDoJob.traitContainer.HasTrait("Berserked")) {
                    return false;
                }
                ApplyAgoraphobicEffect(characterThatWillDoJob);
                return true;
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            //string successLogKey = base.TriggerFlaw(character);
            //if (character.homeStructure != null) {
            //    if (character.currentStructure != character.homeStructure) {
            //        if (character.currentActionNode != null) {
            //            character.StopCurrentActionNode(false);
            //        }
            //        if (character.stateComponent.currentState != null) {
            //            character.stateComponent.ExitCurrentState();
            //        }
            //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
            //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, character);
            //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
            //        goapPlan.SetDoNotRecalculate(true);
            //        job.SetCannotBePushedBack(true);
            //        job.SetAssignedPlan(goapPlan);
            //        character.jobQueue.AddJobInQueue(job);
            //        return successLogKey;
            //    } else {
            //        return "fail_at_home";
            //    }
            //} else {
            //    return "fail_no_home";
            //}
            ApplyAgoraphobicEffect(character, JOB_TYPE.TRIGGER_FLAW);
            return base.TriggerFlaw(character);

        }
        #endregion

        private void ApplyAgoraphobicEffect(Character character, JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME/*, bool processCombat*/) {
            if (!character.canPerform || !character.canWitness) {
                return;
            }
            if(!WillTriggerAgoraphobia(character)) {
                return;
            }
            character.StopCurrentActionNode(false);
            character.jobQueue.CancelAllJobs();
            character.traitContainer.AddTrait(character, "Anxious");
            if(character.homeStructure != null && character.currentStructure != character.homeStructure) {
                if (!character.jobComponent.TriggerFleeHome(jobType)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                }
            } else {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
            }
            //character.marker.AddAvoidsInRange(character.marker.inVisionCharacters, processCombat, "agoraphobia");
            //character.needsComponent.AdjustHappiness(-50);
            //character.needsComponent.AdjustTiredness(-150);
        }
        private bool WillTriggerAgoraphobia(Character character) {
            int count = 0;
            if (character.marker.inVisionCharacters.Count >= 3) {
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    if (!character.marker.inVisionCharacters[i].isDead) {
                        count++;
                    }
                }
            }
            return count >= 3;
        }
    }
}

