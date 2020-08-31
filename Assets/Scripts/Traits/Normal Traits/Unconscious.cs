﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Unconscious : Status {
        public override bool isSingleton => true;

        public Unconscious() {
            name = "Unconscious";
            description = "Knocked out!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3); //144
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_UNCONSCIOUS };
            hindersWitness = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Overrides
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return descriptionInUI;
            }
            return $"This character has been knocked out by {responsibleCharacter.name}";
        }
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                //_sourceCharacter = character;
                character.needsComponent.AdjustDoNotGetTired(1);
                if (character.currentHP <= 0) {
                    character.SetHP(1);
                }
                //CheckToApplyRestrainJob();
                //_sourceCharacter.CreateRemoveTraitJob(name);
                character.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoing == null) { //TODO: || gainedFromDoing.poiTarget != _sourceCharacter
                    character.RegisterLog("NonIntel", "add_trait", null, name.ToLower());
                } 
                //else {
                    //Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait");
                    //addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);


                    //if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
                    //    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //} else if (gainedFromDoing.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                    //    gainedFromDoing.states["Knockout Success"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //}
                //}
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if(sourceCharacter is Character character) {
                if (character.isDead == false) {
                    character.AdjustHP(1, ELEMENTAL_TYPE.Normal);
                }
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.RemoveTraitNeededToBeRemoved(this);
                character.RegisterLog("NonIntel", "remove_trait", null, name.ToLower());
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool OnDeath(Character character) {
            //base.OnDeath(character);
            return character.traitContainer.RemoveTrait(character, this);
        }
        //public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        //    if (traitOwner is Character) {
        //        Character targetCharacter = traitOwner as Character;
        //        if (!targetCharacter.isDead && targetCharacter.faction == characterThatWillDoJob.faction && !targetCharacter.isCriminal && characterThatWillDoJob.isSerialKiller) {
        //            SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Psychopath") as SerialKiller;
        //            serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
        //            return false;
        //            //if (serialKiller != null) {
        //            //    serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
        //            //    return false;
        //            //}
        //        }
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        //}
        public override void OnTickStarted(ITraitable traitable) {
            base.OnTickStarted(traitable);
            if (traitable is Character character) {
                character.needsComponent.AdjustTiredness(1.4f);
            }
        }
        public override void OnHourStarted(ITraitable traitable) {
            base.OnHourStarted(traitable);
            if (traitable is Character character) {
                CheckForLycanthropy(character);
            }
        }
        #endregion

        #region Lycanthropy
        private void CheckForLycanthropy(Character character) {
            if(character.isLycanthrope) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 25) {
                    character.lycanData.Transform(character);
                }
            }
        }
        #endregion
    }
}
