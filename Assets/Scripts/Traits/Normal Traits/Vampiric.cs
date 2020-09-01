using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class Vampiric : Trait {
        public override bool isSingleton => true;

        public Vampiric() {
            name = "Vampiric";
            description = "Sustains itself by drinking other's blood.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Execute_Expected_Effect_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT, JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT);
                character.needsComponent.SetTirednessForcedTick(0);
                character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.AdjustDoNotGetTired(1);
                character.needsComponent.ResetTirednessMeter();
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
                character.needsComponent.SetTirednessForcedTick();
                character.needsComponent.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.AdjustDoNotGetTired(-1);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is Character) {
        //        //In Vampiric, the parameter traitOwner is the target character, that's why you must pass the target character in this parameter not the actual owner of the trait, the actual owner of the trait is the characterThatWillDoJob
        //        //Character targetCharacter = targetPOI as Character;
        //        //if (characterThatWillDoJob.currentActionNode.action != null && characterThatWillDoJob.currentActionNode.action.goapType == INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD && !characterThatWillDoJob.currentActionNode.isDone) {
        //        //    if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE && targetCharacter.traitContainer.GetNormalTrait<Trait>("Vampiric") == null && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
        //        //        //TODO: GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(characterThatWillDoJob.currentJobNode.jobType, INTERACTION_TYPE.DRINK_BLOOD, targetCharacter);
        //        //        //job.SetIsStealth(true);
        //        //        //characterThatWillDoJob.currentActionNode.action.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentActionNode.action.parentPlan.job);
        //        //        //characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
        //        //        //characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
        //        //        return true;
        //        //    }
        //        //}
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                //if (character.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT)) {
                //    character.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
                //}

                Character targetCharacter = GetDrinkBloodTarget(character);
                if(targetCharacter != null) {
                    bool triggerGrieving = false;
                    Griefstricken griefstricken = character.traitContainer.GetNormalTrait<Griefstricken>("Griefstricken");
                    if (griefstricken != null) {
                        triggerGrieving = UnityEngine.Random.Range(0, 100) < (25 * character.traitContainer.stacks[griefstricken.name]);
                    }
                    if (!triggerGrieving) {
                        // GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK_BLOOD, character, character);
                        character.jobComponent.CreateDrinkBloodJob(JOB_TYPE.TRIGGER_FLAW, targetCharacter);
                        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), character, character);
                        //character.jobQueue.AddJobInQueue(job);
                    } else {
                        griefstricken.TriggerGrieving();
                    }
                } else {
                    return "no_victim";
                }
                
            } else {
                return "has_trigger_flaw";
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.DRINK_BLOOD) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter && targetCharacter.advertisedActions.Contains(INTERACTION_TYPE.DRINK_BLOOD) && characterThatWillDoJob.needsComponent.isStarving) {
                if (!characterThatWillDoJob.relationshipContainer.IsFriendsWith(targetCharacter) &&
                    !characterThatWillDoJob.relationshipContainer.IsFamilyMember(targetCharacter) && 
                    !characterThatWillDoJob.relationshipContainer.HasSpecialPositiveRelationshipWith(targetCharacter)) {
                    characterThatWillDoJob.jobComponent.CreateDrinkBloodJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, targetCharacter);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion

        private Character GetDrinkBloodTarget(Character vampire) {
            List<Character> targets = null;
            if(vampire.currentRegion != null) {
                for (int i = 0; i < vampire.currentRegion.charactersAtLocation.Count; i++) {
                    Character character = vampire.currentRegion.charactersAtLocation[i];
                    if(vampire != character) {
                        if(!character.traitContainer.HasTrait("Vampiric") && character.isNormalCharacter && character.Advertises(INTERACTION_TYPE.DRINK_BLOOD) && !character.isDead && !vampire.relationshipContainer.IsFriendsWith(character)
                            && vampire.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation)) {
                            if(targets == null) { targets = new List<Character>(); }
                            targets.Add(character);
                        }
                    }
                }
            }
            if(targets != null && targets.Count > 0) {
                return UtilityScripts.CollectionUtilities.GetRandomElement(targets);  
            }
            return null;
        }
    }
}

