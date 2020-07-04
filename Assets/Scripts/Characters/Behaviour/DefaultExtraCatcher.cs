using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class DefaultExtraCatcher : CharacterBehaviourComponent {
    public DefaultExtraCatcher() {
        priority = 0;
        //attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is in default extra catcher behaviour";
        //if((character.homeStructure == null || character.homeStructure.hasBeenDestroyed) && !character.HasTerritory()) {
        //    log += "\n-Character does not have home structure or territory, 25% chance to set home";
        //    if(UnityEngine.Random.Range(0, 100) < 25) {
        //        log += "\n-Character will set home";
        //        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
        //    }
        //}
        producedJob = null;
        if (character.isNormalCharacter && character.marker != null && character.marker.inVisionCharacters.Count > 0 && HasCharacterNotConversedInMinutes(character, 10)) {
            log += $"\n{character.name} has characters in vision and has not conversed in at least 10 minutes.";
            List<Character> validChoices =
                character.marker.GetInVisionCharactersThatMeetCriteria((c) => HasCharacterNotConversedInMinutes(c, 10) && c.isNormalCharacter);
            if (validChoices != null) {
                Character chosenTarget = CollectionUtilities.GetRandomElement(validChoices);
                log += $"\n{character.name} has characters in vision that have not conversed in at least 10 minutes. Chosen target is {chosenTarget.name}. Rolling chat chance";
                if (GameUtilities.RollChance(20, ref log)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, chosenTarget);
                    return true;
                }
                else {
                    log += $"\nChat roll failed.";
                    if (character.moodComponent.moodState == MOOD_STATE.NORMAL && 
                        RelationshipManager.IsSexuallyCompatible(character.sexuality, chosenTarget.sexuality, 
                            character.gender, chosenTarget.gender) && 
                        character.relationshipContainer.IsFamilyMember(chosenTarget) == false) {
                        log += "\nCharacter is in normal mood and is sexually compatible with target and target is not from same family tree";
                        
                        if (character.relationshipContainer.HasRelationshipWith(chosenTarget, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)
                            || character.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER) == -1
                            || character.traitContainer.HasTrait("Unfaithful")) {
                            log += "\nCharacter does not have a lover or target is the lover or affair of character or character is unfaithful.";
                            
                            log += "\n-Flirt has 1% (multiplied by Compatibility value) chance to trigger";
                            int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(character, chosenTarget);
                            int baseChance = 1;
                            if (character.moodComponent.moodState == MOOD_STATE.NORMAL) {
                                log += "\n-Flirt has +2% chance to trigger because character is in a normal mood";
                                baseChance += 2;
                            }

                            int flirtChance;
                            if (compatibility != -1) {
                                //has compatibility value
                                flirtChance = baseChance * compatibility;
                                log += $"\n-Chance: {flirtChance.ToString()}";
                            } else {
                                //has NO compatibility value
                                flirtChance = baseChance * 2;
                                log += $"\n-Chance: {flirtChance.ToString()} (No Compatibility)";
                            }
                            
                            if (GameUtilities.RollChance(flirtChance, ref log)) {
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, chosenTarget);
                                return true;
                            } else {
                                log += "\n-Flirt did not trigger";
                            }
                        } else {
                            log += "\n-Flirt did not trigger";
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
