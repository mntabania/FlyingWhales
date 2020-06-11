using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Restrained : Status {
        private Character owner;
        //private bool _createdFeedJob;

        public bool isPrisoner { get; private set; }
        //public bool isCriminal { get; private set; }
        //public bool isLeader { get; private set; }

        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}

        public Restrained() {
            name = "Restrained";
            description = "This character is restrained!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, INTERACTION_TYPE.REMOVE_RESTRAINED };
            ticksDuration = 0;
            hindersMovement = true;
            hindersAttackTarget = true;
            hindersPerform = true;
        }

        #region Overrides
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
            }
            return $"This character is restrained by {responsibleCharacter.name}";
        }
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                //isCriminal = _sourceCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL);
                //isLeader = _sourceCharacter.role.roleType == CHARACTER_ROLE.LEADER;
                //Messenger.AddListener(Signals.TICK_STARTED, CheckRestrainTrait);
                //Messenger.AddListener(Signals.HOUR_STARTED, CheckRestrainTraitPerHour);
                //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_restrained");
                //_sourceCharacter.RemoveTrait("Unconscious", removedBy: responsibleCharacter);
                //_sourceCharacter.CancelAllJobsAndPlans();
                owner.AddTraitNeededToBeRemoved(this);
                //Once a faction leader is restrained set new faction leader
                //if (isLeader) {
                //    _sourceCharacter.faction.SetNewLeader();
                //}
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.FEED);
                character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
                // if(!(removedBy != null && removedBy.currentActionNode.action.goapType == INTERACTION_TYPE.JUDGE_CHARACTER && removedBy.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)) {
                //     character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
                // }
                //Messenger.RemoveListener(Signals.TICK_STARTED, CheckRestrainTrait);
                //Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRestrainTraitPerHour);
                //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
                owner.RemoveTraitNeededToBeRemoved(this);
                // Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND, owner as IPointOfInterest);
                //If restrained trait is removed from this character this means that the character is set free from imprisonment, either he/she was saved from abduction or freed from criminal charges
                //When this happens, check if he/she was the leader of the faction, if true, he/she can only go back to being the ruler if he/she was not imprisoned because he/she was a criminal
                //But if he/she was a criminal, he/she cannot go back to being the ruler
                //if (isLeader && !isCriminal) {
                //    _sourceCharacter.faction.SetLeader(character);

                //    Log logNotif = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "return_faction_leader");
                //    logNotif.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    logNotif.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                //    _sourceCharacter.AddHistory(logNotif);
                //    PlayerManager.Instance.player.ShowNotification(logNotif);
                //}
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (targetCharacter.isDead) {
                    return false;
                }
                if (!targetCharacter.traitContainer.HasTrait("Criminal")) {
                    if (characterThatWillDoJob.traitContainer.HasTrait("Psychopath")) {
                        //SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Psychopath") as SerialKiller;
                        //serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                        return false;
                        //if (serialKiller != null) {
                        //    serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                        //    return false;
                        //}
                    }
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS, name);
                    if (currentJob == null) {
                        if (!IsResponsibleForTrait(characterThatWillDoJob) && InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter)) {
                            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, targetCharacter, characterThatWillDoJob);
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
        #endregion
        
        // private void CreateJudgementJob() {
        //     if (!owner.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
        //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, owner, owner.currentNpcSettlement);
        //         job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
        //         job.SetStillApplicableChecker(() => InteractionManager.Instance.IsJudgementJobStillApplicable(owner));
        //         owner.currentNpcSettlement.AddToAvailableJobs(job);
        //     }
        // }
        public void SetIsPrisoner(bool state) {
            if(isPrisoner != state) {
                isPrisoner = state;
                if (isPrisoner) {
                    // CreateJudgementJob();
                    Criminal criminalTrait = owner.traitContainer.GetNormalTrait<Criminal>("Criminal");
                    criminalTrait?.crimeData.SetCrimeStatus(CRIME_STATUS.Imprisoned);
                }
            }
        }
    }

}
