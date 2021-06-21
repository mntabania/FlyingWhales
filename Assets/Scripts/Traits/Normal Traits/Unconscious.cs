using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Unconscious : Status {
        //Not singleton for responsible characters
        //public override bool isSingleton => true;

        public Unconscious() {
            name = "Unconscious";
            description = "Knocked out!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(3); //144
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_UNCONSCIOUS };

            //The reason why unconscious does not hinder movement is so that the actor will restrain the character first if it is going to apprehend him
            //If the unconscious will hinder movement the plan will no longer include restraining the target, so it means that while the target is being carried, there is a high possibility the he will wake up, this will result into other issues
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
                if (!character.HasHealth()) {
                    character.SetHP(1);
                }
                //CheckToApplyRestrainJob();
                //_sourceCharacter.CreateRemoveTraitJob(name);
                character.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoingType == INTERACTION_TYPE.NONE) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", null, LOG_TAG.Needs);
                    log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase(true);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if(sourceCharacter is Character character) {
                if (character.isDead == false) {
                    character.AdjustHP(1, ELEMENTAL_TYPE.Normal);
                }
                character.needsComponent.AdjustDoNotGetTired(-1);
                character.RemoveTraitNeededToBeRemoved(this);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "remove_trait", null, LOG_TAG.Needs);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase(true);
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
            if(character.isLycanthrope && !character.lycanData.isMaster) {
                if (ChanceData.RollChance(CHANCE_TYPE.Lycanthrope_Transform_Chance)) {
                    character.lycanData.Transform(character);
                }
            }
        }
        #endregion
    }
}
