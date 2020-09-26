using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class FactionLeaderBehaviour : CharacterBehaviourComponent {
    public FactionLeaderBehaviour() {
        priority = 20;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is a faction leader";
        Faction faction = character.faction;
        if(faction != null && faction.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) && !faction.HasJob(JOB_TYPE.RAID)) {
            log += $"\n-10% chance to declare raid";
            int roll = UnityEngine.Random.Range(0, 100);
            if(roll < 10) {
                if (!faction.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid)) {
                    log += $"\n-Character faction is warmonger and has no raid job and has no raid party yet";
                    Faction targetFaction = faction.GetRandomAtWarFaction();
                    if (targetFaction != null) {
                        log += $"\n-Chosen target faction: " + targetFaction.name;
                        BaseSettlement targetSettlement = targetFaction.GetRandomOwnedSettlement();
                        if (targetSettlement != null) {
                            log += $"\n-Chosen target settlement: " + targetSettlement.name;
                            LocationStructure targetStructure = targetSettlement.GetRandomStructure();
                            if (targetSettlement is NPCSettlement npcSettlement && npcSettlement.cityCenter != null) {
                                targetStructure = npcSettlement.cityCenter;
                            }
                            character.interruptComponent.SetRaidTargetSettlement(targetSettlement);
                            if (character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_Raid, character)) {
                                producedJob = null;
                                return true;
                            }
                        }
                    }
                }
            }
        }
        if (character.homeSettlement != null && character.homeSettlement.prison != null) {
            LocationStructure structure = character.homeSettlement.prison;
            log += $"\n-15% chance to recruit a restrained character from different faction";
            int roll = Random.Range(0, 100);
            log += $"\n-Roll: {roll}";
            if (roll < 15) {
                Character targetCharacter = structure.GetRandomCharacterThatMeetCriteria(x => x.traitContainer.HasTrait("Restrained") && x.faction != character.faction && !x.HasJobTargetingThis(JOB_TYPE.RECRUIT));
                if(targetCharacter != null) {
                    log += $"\n-Chosen target: {targetCharacter.name}";
                    return character.jobComponent.TriggerRecruitJob(targetCharacter, out producedJob);
                }
            }
        }
        producedJob = null;
        return false;
    }
}
