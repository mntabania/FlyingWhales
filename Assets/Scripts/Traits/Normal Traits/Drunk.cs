using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Drunk : Status {
        public override bool isSingleton => true;

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
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            Character owner = addedTo as Character;
            owner.movementComponent.AdjustSpeedModifier(-0.4f);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            Character owner = removedFrom as Character;
            owner.movementComponent.AdjustSpeedModifier(0.4f);
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter) {
                if (!targetCharacter.isDead && characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                    int chance = 0;
                    if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, BaseRelationshipContainer.Enemy)) {
                        chance = 10;
                    } else if (characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, BaseRelationshipContainer.Rival)) {
                        chance = 25;
                    } else {
                        Debug.LogWarning($"There is no drunk combat chance case for character {characterThatWillDoJob.name}!");
                    }
                    
                    int roll = UnityEngine.Random.Range(0, 100);
                    if (roll < chance) {
                        if (!targetCharacter.traitContainer.HasTrait("Unconscious")) {
                            if (characterThatWillDoJob.combatComponent.Fight(targetCharacter, CombatManager.Drunk, isLethal: false)) {
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
