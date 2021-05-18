using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UtilityScripts;

public class DefaultExtraCatcher : CharacterBehaviourComponent {
    public DefaultExtraCatcher() {
        priority = 0;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log = $"{log}\n-{character.name} is in default extra catcher behaviour";
#endif
        //if((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
        //    log += "\n-Character does not have home structure or territory, 25% chance to set home";
        //    if(UnityEngine.Random.Range(0, 100) < 25) {
        //        log += "\n-Character will set home";
        //        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
        //    }
        //}
        producedJob = null;
        if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && CharacterManager.Instance.HasCharacterNotConversedInMinutes(character, 6)) {
#if DEBUG_LOG
            log = $"{log}\n{character.name} has characters in vision and has not conversed in at least 10 minutes.";
#endif
            List<Character> validChoices = RuinarchListPool<Character>.Claim();
            character.marker.PopulateCharactersThatIsNotDeadVillagerAndNotConversedInMinutes(validChoices, 6);
            Character chosenTarget = null;
            if (validChoices.Count > 0) {
                chosenTarget = CollectionUtilities.GetRandomElement(validChoices);
            }
            RuinarchListPool<Character>.Release(validChoices);
            if (chosenTarget != null) {
#if DEBUG_LOG
                log = $"{log}\n{character.name} has characters in vision that have not conversed in at least 10 minutes. Chosen target is {chosenTarget.name}. Rolling chat chance";
#endif
                if (character.nonActionEventsComponent.CanChat(chosenTarget) && GameUtilities.RollChance(20, ref log)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenTarget);
                    return true;
                } else {
#if DEBUG_LOG
                    log = $"{log}\nChat roll failed.";
#endif
                    if (character.moodComponent.moodState == MOOD_STATE.Normal && RelationshipManager.Instance.IsCompatibleBasedOnSexualityAndOpinion(character, chosenTarget) && character.limiterComponent.isSociable) { // && !character.relationshipContainer.IsFamilyMember(chosenTarget)
#if DEBUG_LOG
                        log = $"{log}\nCharacter is in normal mood and is compatible with target";
#endif
                        if (character.nonActionEventsComponent.CanFlirt(character, chosenTarget)) {
#if DEBUG_LOG
                            log = $"{log}\nCharacter can flirt with target.";
#endif
                            int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(character, chosenTarget);
                            int baseChance = ChanceData.GetChance(CHANCE_TYPE.Flirt_On_Sight_Base_Chance);
#if DEBUG_LOG
                            log = $"{log}\n-Flirt has {baseChance}% (multiplied by Compatibility value) chance to trigger";
#endif
                            if (character.moodComponent.moodState == MOOD_STATE.Normal) {
#if DEBUG_LOG
                                log = $"{log}\n-Flirt has +2% chance to trigger because character is in a normal mood";
#endif
                                baseChance += 2;
                            }

                            int flirtChance;
                            if (compatibility != -1) {
                                //has compatibility value
                                flirtChance = baseChance * compatibility;
#if DEBUG_LOG
                                log = $"{log}\n-Chance: {flirtChance.ToString()}";
#endif
                            } else {
                                //has NO compatibility value
                                flirtChance = baseChance * 2;
#if DEBUG_LOG
                                log = $"{log}\n-Chance: {flirtChance.ToString()} (No Compatibility)";
#endif
                            }

                            if (GameUtilities.RollChance(flirtChance, ref log)) {
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, chosenTarget);
                                return true;
                            } else {
#if DEBUG_LOG
                                log = $"{log}\n-Flirt did not trigger";
#endif
                            }
                        } else {
#if DEBUG_LOG
                            log = $"{log}\n-Flirt did not trigger";
#endif
                        }
                    }
                }
            }
        }
#if DEBUG_LOG
        log += "\n-Chat and flirt did not trigger. Will create an Idle Stand job";
#endif
        return character.jobComponent.TriggerStand(out producedJob);
    }
}
