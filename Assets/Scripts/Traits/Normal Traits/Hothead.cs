using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Hothead : Trait {
        public override bool isSingleton => true;

        public Hothead() {
            name = "Hothead";
            description = "Quick to anger. May flare up when seeing an enemy. If afflicted by the player, will produce a Chaos Orb each time it gets Angry.";
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
                string debugLog = string.Empty;
#if DEBUG_LOG
                debugLog = $"Hotheaded reaction: {characterThatWillDoJob.name} saw {targetCharacter.name} and has {name}";
#endif
                // int chance = UnityEngine.Random.Range(0, 100);
                float chance = PlayerSkillManager.Instance.GetTriggerRateForCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED);
#if DEBUG_LOG
                debugLog = $"{debugLog}\n-{chance.ToString("F2")} chance to trigger Angered interrupt if saw a character";
#endif
                if (GameUtilities.RollChance(chance, ref debugLog)) {
                    bool isRelationshipRequirementMet = false;
                    List<OPINIONS> validOpinions = RuinarchListPool<OPINIONS>.Claim();
                    PlayerSkillManager.Instance.PopulateOpinionTriggersAtCurrentLevel(PLAYER_SKILL_TYPE.HOTHEADED, validOpinions);
                    if (validOpinions.Contains(OPINIONS.NoOne) && validOpinions.Count == 1) {
                        isRelationshipRequirementMet = false;
                    } else if (validOpinions.Contains(OPINIONS.Everyone)) {
                        isRelationshipRequirementMet = true;
                    } else {
                        isRelationshipRequirementMet = characterThatWillDoJob.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, validOpinions);
                    }
                    if (isRelationshipRequirementMet) {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Character considers Target as {validOpinions.ComafyList()}, will trigger Angered interrupt";
                        characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
#endif
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Angered, targetCharacter);
                        return true;
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Character does not consider Target as {validOpinions.ComafyList()}";
#endif
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
#if DEBUG_LOG
                characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
#endif
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
#endregion
    }

}
