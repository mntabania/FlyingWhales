using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;

namespace Traits {
    public class Prisoner : Status {
        private Character owner;

        public Faction prisonerOfFaction { get; private set; }
        public Character prisonerOfCharacter { get; private set; }

        #region getters
        public override Type serializedData => typeof(SaveDataPrisoner);
        public bool isFactionPrisoner => prisonerOfFaction != null;
        public bool isPersonalPrisoner => prisonerOfCharacter != null;
        public bool isPrisoner => isFactionPrisoner || isPersonalPrisoner;
        #endregion

        public Prisoner() {
            name = "Prisoner";
            description = "Imprisoned";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //isHidden = true;
        }

        #region Loading
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character) {
                owner = addTo as Character;
            }
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            SaveDataPrisoner saveDataPrisoner = saveDataTrait as SaveDataPrisoner;
            Assert.IsNotNull(saveDataPrisoner);
            if (!string.IsNullOrEmpty(saveDataPrisoner.prisonerOfFaction)) {
                prisonerOfFaction = FactionManager.Instance.GetFactionByPersistentID(saveDataPrisoner.prisonerOfFaction);
            }
            if (!string.IsNullOrEmpty(saveDataPrisoner.prisonerOfCharacter)) {
                prisonerOfCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataPrisoner.prisonerOfCharacter);
            }
        }
        #endregion

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            data = $"{data}Prisoner of faction: {prisonerOfFaction?.name}";
            data = $"{data}\nPrisoner of character: {prisonerOfCharacter?.name}";
            return data;
        }
        //public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        //    if (sourceCharacter is Character character) {
        //        character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.FEED);
        //        character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
        //        // if(!(removedBy != null && removedBy.currentActionNode.action.goapType == INTERACTION_TYPE.JUDGE_CHARACTER && removedBy.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING)) {
        //        //     character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.JUDGE_PRISONER);
        //        // }
        //        //Messenger.RemoveListener(Signals.TICK_STARTED, CheckRestrainTrait);
        //        //Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRestrainTraitPerHour);
        //        //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        //        owner.RemoveTraitNeededToBeRemoved(this);
        //        // Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND, owner as IPointOfInterest);
        //        //If restrained trait is removed from this character this means that the character is set free from imprisonment, either he/she was saved from abduction or freed from criminal charges
        //        //When this happens, check if he/she was the leader of the faction, if true, he/she can only go back to being the ruler if he/she was not imprisoned because he/she was a criminal
        //        //But if he/she was a criminal, he/she cannot go back to being the ruler
        //        //if (isLeader && !isCriminal) {
        //        //    _sourceCharacter.faction.SetLeader(character);
        //        //    Log logNotif = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "return_faction_leader");
        //        //    logNotif.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        //    logNotif.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
        //        //    _sourceCharacter.AddHistory(logNotif);
        //        //    PlayerManager.Instance.player.ShowNotification(logNotif);
        //        //}
        //        character.traitContainer.RemoveTrait(character, "Webbed"); //always remove webbed trait after restrained has been removed

        //        //always set character as un-abducted by anyone after they lose restrain trait. 
        //        character.defaultCharacterTrait.SetHasBeenAbductedByWildMonster(false);
        //        character.defaultCharacterTrait.SetHasBeenAbductedByPlayerMonster(false);
        //    }
        //    base.OnRemoveTrait(sourceCharacter, removedBy);
        //}
        //public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        //    if (traitOwner is Character) {
        //        Character targetCharacter = traitOwner as Character;
        //        if (targetCharacter.isDead) {
        //            return false;
        //        }
        //        if (!targetCharacter.traitContainer.HasTrait("Criminal")) {
        //            if (characterThatWillDoJob.traitContainer.HasTrait("Psychopath")) {
        //                //Psychopath psychopath = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Psychopath") as Psychopath;
        //                //psychopath.PsychopathSawButWillNotAssist(targetCharacter, this);
        //                return false;
        //                //if (psychopath != null) {
        //                //    psychopath.PsychopathSawButWillNotAssist(targetCharacter, this);
        //                //    return false;
        //                //}
        //            }
        //            GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_STATUS, name);
        //            if (currentJob == null) {
        //                if (!IsResponsibleForTrait(characterThatWillDoJob) && InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter)) {
        //                    GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
        //                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, targetCharacter, characterThatWillDoJob);
        //                    // job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.TOOL });
        //                    // job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.TOOL].craftCost });
        //                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
        //                    return true;
        //                }
        //            } 
        //            //else {
        //            //    if (InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, currentJob)) {
        //            //        return TryTransferJob(currentJob, characterThatWillDoJob);
        //            //    }
        //            //}
        //        }
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        //}
        #endregion

        public bool IsFactionPrisonerOf(Faction faction) {
            if (isFactionPrisoner) {
                return prisonerOfFaction == faction;
            }
            return false;
        }
        public bool IsPersonalPrisonerOf(Character character) {
            if (isPersonalPrisoner) {
                return prisonerOfCharacter == character;
            }
            return false;
        }
        public bool IsConsideredPrisonerOf(Character character) {
            if (IsPersonalPrisonerOf(character)) {
                return true;
            } else {
                if (character.faction != null) {
                    if (IsFactionPrisonerOf(character.faction)) {
                        return true;
                    }
                }
            }
            return false;
        }
        public void SetPrisonerOfFaction(Faction faction) {
            prisonerOfFaction = faction;
        }
        public void SetPrisonerOfCharacter(Character character) {
            prisonerOfCharacter = character;
        }

    }
}

#region Save Data
public class SaveDataPrisoner : SaveDataTrait {
    public string prisonerOfFaction;
    public string prisonerOfCharacter;

    public override void Save(Trait trait) {
        base.Save(trait);
        Prisoner prisoner = trait as Prisoner;
        Assert.IsNotNull(prisoner);
        if (prisoner.isFactionPrisoner) {
            prisonerOfFaction = prisoner.prisonerOfFaction.persistentID;
        }
        if (prisoner.isPersonalPrisoner) {
            prisonerOfCharacter = prisoner.prisonerOfCharacter.persistentID;
        }
    }
}
#endregion