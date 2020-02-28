using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Drunk : Trait {

        public Drunk() {
            name = "Drunk";
            description = "This character is intoxicated and may lash out at nearby characters.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = 4;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            Character owner = addedTo as Character;
            owner.AdjustSpeedModifier(-0.4f);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            Character owner = removedFrom as Character;
            owner.AdjustSpeedModifier(0.4f);
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.isDead && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                    // int value = 1;
                    // RELATIONSHIP_EFFECT relEffect = characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter);
                    // if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE) {
                    //     value = 3;
                    // } else if (relEffect == RELATIONSHIP_EFFECT.NONE) {
                    //     value = 2;
                    // }
                    // if (characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
                    //     value *= 5;
                    // }
                    int chance = UnityEngine.Random.Range(0, 100);
                    if (chance < 25) {
                        // characterThatWillDoJob.logComponent.PrintLogIfActive(characterThatWillDoJob.name
                        //                                                      + " Drunk Assault Chance: " + value + ", Roll: " + chance);
                        //if (characterThatWillDoJob.combatComponent.AddHostileInRange(targetCharacter, false, false)) {
                        //    if (!characterThatWillDoJob.combatComponent.avoidInRange.Contains(targetCharacter)) {
                        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "drunk_assault");
                        //        log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        //        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //        //log.AddLogToInvolvedObjects();
                        //        characterThatWillDoJob.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        //    }
                        //    //characterThatWillDoJob.combatComponent.ProcessCombatBehavior();
                        //}
                        if (!targetCharacter.traitContainer.HasTrait("Unconscious")) {
                            if (characterThatWillDoJob.combatComponent.Fight(targetCharacter, false)) {
                                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "drunk_assault");
                                log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                                characterThatWillDoJob.logComponent.RegisterLog(log, onlyClickedCharacter: false);
                            }
                        }
                        return true;
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}
