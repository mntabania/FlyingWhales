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
        log = $"{log}\n-{character.name} is in default extra catcher behaviour";
        //if((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
        //    log += "\n-Character does not have home structure or territory, 25% chance to set home";
        //    if(UnityEngine.Random.Range(0, 100) < 25) {
        //        log += "\n-Character will set home";
        //        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
        //    }
        //}
        producedJob = null;
        if (character.isNormalCharacter && character.hasMarker && character.marker.inVisionCharacters.Count > 0 && HasCharacterNotConversedInMinutes(character, 6)) {
            log = $"{log}\n{character.name} has characters in vision and has not conversed in at least 10 minutes.";
            List<Character> validChoices = character.marker.GetInVisionCharactersThatMeetCriteria((c) => HasCharacterNotConversedInMinutes(c, 6) && c.isNormalCharacter && !c.isDead);
            if (validChoices != null && validChoices.Count > 0) {
                Character chosenTarget = CollectionUtilities.GetRandomElement(validChoices);
                log = $"{log}\n{character.name} has characters in vision that have not conversed in at least 10 minutes. Chosen target is {chosenTarget.name}. Rolling chat chance";
                if (character.nonActionEventsComponent.CanChat(chosenTarget) && GameUtilities.RollChance(20, ref log)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenTarget);
                    return true;
                } else {
                    log = $"{log}\nChat roll failed.";
                    if (character.moodComponent.moodState == MOOD_STATE.Normal && RelationshipManager.Instance.IsCompatibleBasedOnSexualityAndOpinion(character, chosenTarget) && character.limiterComponent.isSociable) { // && !character.relationshipContainer.IsFamilyMember(chosenTarget)
                        log = $"{log}\nCharacter is in normal mood and is compatible with target";

                        if (character.nonActionEventsComponent.CanFlirt(character, chosenTarget)) {
                            log = $"{log}\nCharacter can flirt with target.";

                            int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(character, chosenTarget);
                            int baseChance = ChanceData.GetChance(CHANCE_TYPE.Flirt_On_Sight_Base_Chance);
                            log = $"{log}\n-Flirt has {baseChance}% (multiplied by Compatibility value) chance to trigger";
                            if (character.moodComponent.moodState == MOOD_STATE.Normal) {
                                log = $"{log}\n-Flirt has +2% chance to trigger because character is in a normal mood";
                                baseChance += 2;
                            }

                            int flirtChance;
                            if (compatibility != -1) {
                                //has compatibility value
                                flirtChance = baseChance * compatibility;
                                log = $"{log}\n-Chance: {flirtChance.ToString()}";
                            } else {
                                //has NO compatibility value
                                flirtChance = baseChance * 2;
                                log = $"{log}\n-Chance: {flirtChance.ToString()} (No Compatibility)";
                            }

                            if (GameUtilities.RollChance(flirtChance, ref log)) {
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, chosenTarget);
                                return true;
                            } else {
                                log = $"{log}\n-Flirt did not trigger";
                            }
                        } else {
                            log = $"{log}\n-Flirt did not trigger";
                        }
                    }
                }
                
            }
        }
        log += "\n-Chat and flirt did not trigger. Will create an Idle Stand job";
        return character.jobComponent.TriggerStand(out producedJob);
    }

    private bool HasCharacterNotConversedInMinutes(Character character, int minutes) {
        GameDate lastConversationDate = character.nonActionEventsComponent.lastConversationDate;
        //add ticks (based on given minutes) to last conversation date. If resulting date is before today, then character
        //has not conversed for the given amount of time.
        return lastConversationDate.AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(minutes))
            .IsBefore(GameManager.Instance.Today());
    }
}
