using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Restrained : Status {
        private Character owner;
        //private bool _createdFeedJob;

        //public bool isCriminal { get; private set; }
        //public bool isLeader { get; private set; }

        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}

        public Restrained() {
            name = "Restrained";
            description = "Tied up to prevent it from moving.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, INTERACTION_TYPE.REMOVE_RESTRAINED };
            ticksDuration = 0;
            hindersMovement = true;
            hindersAttackTarget = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
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
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return descriptionInUI;
            }
            return $"This character is restrained by {responsibleCharacter.name}";
        }
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                owner.AddTraitNeededToBeRemoved(this);
                //owner.traitContainer.AddTrait(owner, "Prisoner");
                PlayerManager.Instance?.player?.retaliationComponent.OnCharacterRestrained(owner);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character character) {
                character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.FEED);
                character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
                owner.RemoveTraitNeededToBeRemoved(this);
                character.traitContainer.RemoveTrait(character, "Webbed"); //always remove webbed trait after restrained has been removed
                
                //always set character as un-abducted by anyone after they lose restrain trait. 
                character.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(false);
                character.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(false);
                //owner.traitContainer.RemoveTrait(owner, "Prisoner");
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool OnDeath(Character character) {
            return character.traitContainer.RemoveTrait(character, this);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (targetCharacter.isDead) {
                    return false;
                }
                if (!targetCharacter.traitContainer.HasTrait("Criminal") && !characterThatWillDoJob.IsHostileWith(targetCharacter)) {
                    if (characterThatWillDoJob.traitContainer.HasTrait("Psychopath")) {
                        //Psychopath psychopath = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Psychopath") as Psychopath;
                        //psychopath.PsychopathSawButWillNotAssist(targetCharacter, this);
                        return false;
                        //if (psychopath != null) {
                        //    psychopath.PsychopathSawButWillNotAssist(targetCharacter, this);
                        //    return false;
                        //}
                    }
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS, name);
                    if (currentJob == null) {
                        if (!IsResponsibleForTrait(characterThatWillDoJob) && InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter)) {
                            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, targetCharacter, characterThatWillDoJob);
                            UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(characterThatWillDoJob, job, INTERACTION_TYPE.NONE);
                            // job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.TOOL });
                            // job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.TOOL].craftCost });
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                    } 
                    //else {
                    //    if (InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                    //        return TryTransferJob(currentJob, characterThatWillDoJob);
                    //    }
                    //}
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            if(traitable is Character character) {
                CheckForLycanthropy(character);
            }
        }
        #endregion
        
        private void CheckForLycanthropy(Character character) {
            if(character.isLycanthrope && !character.lycanData.isMaster && 
               character.lycanData.activeForm == character.lycanData.lycanthropeForm) {
                //only transform back to human/elf if restrained
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 25) { //25
                    character.lycanData.Transform(character);
                }
            }
        }

        // private void CreateJudgementJob() {
        //     if (!owner.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
        //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, owner, owner.currentNpcSettlement);
        //         job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
        //         job.SetStillApplicableChecker(() => InteractionManager.Instance.IsJudgementJobStillApplicable(owner));
        //         owner.currentNpcSettlement.AddToAvailableJobs(job);
        //     }
        // }
        //public void SetIsPrisoner(bool state) {
        //    if(isPrisoner != state) {
        //        isPrisoner = state;
        //        if (isPrisoner) {
        //            // CreateJudgementJob();

        //        }
        //    }
        //}
    }

}
