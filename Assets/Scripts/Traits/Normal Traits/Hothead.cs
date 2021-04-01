using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Hothead : Trait {
        public override bool isSingleton => true;

        public Hothead() {
            name = "Hothead";
            description = "Quick to anger. May flare up when seeing an enemy.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            character.traitContainer.AddTrait(character, "Angry");
            return base.TriggerFlaw(character);
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter) {
                string debugLog = $"Hotheaded reaction: {characterThatWillDoJob.name} saw {targetCharacter.name} and has {name}";
                // int chance = UnityEngine.Random.Range(0, 100);
                float chance = PlayerSkillManager.Instance.GetTriggerRateForCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED);
                debugLog = $"{debugLog}\n-{chance.ToString("F2")} chance to trigger Angered interrupt if saw a character";
                if (GameUtilities.RollChance(chance, ref debugLog)) {
                    bool isRelationshipRequirementMet = false;
                    List<OPINIONS> validOpinions = RuinarchListPool<OPINIONS>.Claim();
                    PlayerSkillManager.Instance.PopulateOpinionTriggersAtCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED, validOpinions);
                    if (validOpinions.Contains(OPINIONS.NoOne) && validOpinions.Count == 1) {
                        isRelationshipRequirementMet = false;
                    } else if (validOpinions.Contains(OPINIONS.Everyone)) {
                        isRelationshipRequirementMet = true;
                    } else {
                        isRelationshipRequirementMet = characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, validOpinions.ToArray());
                    }
                    if (isRelationshipRequirementMet) {
                        debugLog = $"{debugLog}\n-Character considers Target as {validOpinions.ComafyList()}, will trigger Angered interrupt";
                        characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Angered, targetCharacter);
                        return true;
                    } else {
                        debugLog = $"{debugLog}\n-Character does not consider Target as {validOpinions.ComafyList()}";
                    }
                    RuinarchListPool<OPINIONS>.Release(validOpinions);
                    // if (characterThatWillDoJob.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                    //     debugLog += "\n-Character considers Target as Enemy or Rival, will trigger Angered interrupt";
                    //     characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
                    //     characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Angered, targetCharacter);
                    //     //character.traitContainer.AddTrait(character, "Angry");
                    //     //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "angry_saw");
                    //     //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //     //log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //     //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                    //     return true;
                    // } else {
                    //     debugLog += "\n-Character does not consider Target as Enemy or Rival";
                    // }
                }
                characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }

}
