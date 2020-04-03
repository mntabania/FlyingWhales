using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Unconscious : Status {
        private Character _sourceCharacter;
        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}

        public Unconscious() {
            name = "Unconscious";
            description = "This character is unconscious.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3); //144
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_UNCONSCIOUS };
            //hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Death_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Tick_Started_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Hour_Started_Trait);
        }

        #region Overrides
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
            }
            return $"This character has been knocked out by {responsibleCharacter.name}";
        }
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character character) {
                _sourceCharacter = character;
                _sourceCharacter.needsComponent.AdjustDoNotGetTired(1);
                if (_sourceCharacter.currentHP <= 0) {
                    _sourceCharacter.SetHP(1);
                }
                //CheckToApplyRestrainJob();
                //_sourceCharacter.CreateRemoveTraitJob(name);
                _sourceCharacter.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoing == null) { //TODO: || gainedFromDoing.poiTarget != _sourceCharacter
                    _sourceCharacter.RegisterLog("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait");
                    addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
                    //    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //} else if (gainedFromDoing.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                    //    gainedFromDoing.states["Knockout Success"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //}
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            _sourceCharacter.AdjustHP(1, ELEMENTAL_TYPE.Normal);
            _sourceCharacter.needsComponent.AdjustDoNotGetTired(-1);
            _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLog("NonIntel", "remove_trait", null, name.ToLower());
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
        public override void OnTickStarted() {
            base.OnTickStarted();
            _sourceCharacter.needsComponent.AdjustTiredness(1.4f);
        }
        public override void OnHourStarted() {
            base.OnHourStarted();
            CheckForLycanthropy();
        }
        #endregion

        #region Lycanthropy
        private void CheckForLycanthropy() {
            if(_sourceCharacter.isLycanthrope) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 25) {
                    _sourceCharacter.lycanData.Transform(_sourceCharacter);
                }
            }
        }
        #endregion
    }
}
