using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Hothead : Trait {

        public Hothead() {
            name = "Hothead";
            description = "Hotheads are easy to anger and may have bouts of rage fits.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        //public override string TriggerFlaw(Character character) {
        //    character.traitContainer.AddTrait(character, "Angry");
        //    return base.TriggerFlaw(character);
        //}
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                string debugLog = $"{characterThatWillDoJob.name} saw {targetPOI.name} and has {name}";
                debugLog += "\n-20% chance to trigger Angered interrupt if saw an Enemy or Rival";
                int chance = UnityEngine.Random.Range(0, 100);
                debugLog += $"\n-Roll: {chance}";
                if (chance < 20) {
                    Character targetCharacter = targetPOI as Character;
                    if (characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                        debugLog += "\n-Character considers Target as Enemy or Rival, will trigger Angered interrupt";
                        characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Angered, targetCharacter);
                        //character.traitContainer.AddTrait(character, "Angry");
                        //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "angry_saw");
                        //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        //log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        return true;
                    } else {
                        debugLog += "\n-Character does not consider Target as Enemy or Rival";
                    }
                }
                characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
            //if (targetPOI is Character) {
            //    Character targetCharacter = targetPOI as Character;
            //    if (!targetCharacter.isDead) {
            //        int chance = UnityEngine.Random.Range(0, 100);
            //        if (chance < 2 && characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
            //            characterThatWillDoJob.PrintLogIfActive(GameManager.Instance.TodayLogString() + characterThatWillDoJob.name
            //                + " Hothead Assault Chance: 2, Roll: " + chance);
            //            if (characterThatWillDoJob.combatComponent.AddHostileInRange(targetCharacter, false, false, false)) {
            //                if (!characterThatWillDoJob.combatComponent.avoidInRange.Contains(targetCharacter)) {
            //                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "hothead_assault");
            //                    log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            //                    //log.AddLogToInvolvedObjects();
            //                    characterThatWillDoJob.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            //                }
            //                //characterThatWillDoJob.combatComponent.ProcessCombatBehavior();
            //            }
            //            return true;
            //        }
            //    }
            //}
        }
        #endregion
    }

}
