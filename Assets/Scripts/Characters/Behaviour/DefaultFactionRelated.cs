﻿using System.Collections;
using System.Collections.Generic;
using Locations.Settlements;
using Traits;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;

public class DefaultFactionRelated : CharacterBehaviourComponent {
    public DefaultFactionRelated() {
        priority = 26;
        attributes = new BEHAVIOUR_COMPONENT_ATTRIBUTE[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING/*, BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY*/ };
    }

    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.isVagrantOrFactionless) {
            if(UnityEngine.Random.Range(0, 100) < 15) {
                log += $"\n-{character.name} is factionless, 15% chance to join faction";
                Faction chosenFaction = character.JoinFactionProcessing();
                if (chosenFaction != null) {
                    log += $"\n-Chosen faction to join: {chosenFaction.name}";
                } else {
                    log += "\n-No available faction that the character fits the ideology";
                }
                return true;
            } else {
                log += $"\nDid not meet chance to join faction. Checking if can create faction...";
                if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions) {
                    int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
                    log += $"\nActive villager faction count is {villagerFactionCount.ToString()}";
                    if (villagerFactionCount < 20) {
                        log += $"\nActive villager factions is less than 20, rolling chances";
                        int factionsInRegion = GetFactionsInRegion(character.currentRegion);
                        float createChance = factionsInRegion >= 2 ? 2f : 3f;
                        if (character.traitContainer.HasTrait("Inspiring", "Ambitious")) {
                            createChance = factionsInRegion >= 2 ? 0.5f : 15f;
                        }
                        if (GameUtilities.RollChance(createChance)) {
                            log += $"\nChance met, creating new faction";
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character);
                            return true;
                        }
                    }
                }
            }
        }
        if(!character.isFactionLeader && !character.isSettlementRuler) {
            if(character.faction != null && character.faction.isMajorNonPlayer) {
                int leaveFactionChance = 0;
                if (character.moodComponent.moodState == MOOD_STATE.Bad) {
                    leaveFactionChance += 3;
                } else if (character.moodComponent.moodState == MOOD_STATE.Critical) {
                    leaveFactionChance += 8;
                }
                if (character.traitContainer.HasTrait("Betrayed") && character.faction.leader != null) {
                    Betrayed betrayed = character.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
                    if (betrayed.IsResponsibleForTrait(character.faction.leader as Character)) {
                        leaveFactionChance += 30;
                    }
                }
                if (GameUtilities.RollChance(leaveFactionChance)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
                    return true;
                }
            }
            if (character.traitContainer.HasTrait("Cultist")) {
                log += $"\n-{character.name} is cultist";
                if (character.faction == null || character.faction.factionType.type != FACTION_TYPE.Demon_Cult) {
                    log += $"\n-Character is not part of a demon cult faction, will try to join one";
                    int chance = 0;
                    if (HasFactionWithMemberWithUnoccupiedDwelling(FACTION_TYPE.Demon_Cult)) {
                        log += $"\n-Has demon cult faction with a member that has unoccupied dwelling, +3%";
                        chance += 3;
                    }
                    if (HasFactionWith2Members(FACTION_TYPE.Demon_Cult)) {
                        log += $"\n-Has demon cult faction with 1 or 2 members, +3%";
                        chance += 3;
                    }
                    if (GameUtilities.RollChance(chance)) {
                        log += $"\n-Will join demon cult";
                        character.JoinFactionProcessing();
                    }
                }
            }
        }
        return false;
    }
    private bool HasFactionWithMemberWithUnoccupiedDwelling(FACTION_TYPE factionType) {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if(faction.factionType.type == factionType) {
                if (faction.HasMemberThatIsNotDeadHasHomeSettlementUnoccupiedDwelling()) {
                    return true;
                }
            }
        }
        return false;
    }
    private bool HasFactionWith2Members(FACTION_TYPE factionType) {
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.factionType.type == factionType) {
                int count = faction.GetAliveMembersCount();
                if (count == 1 || count == 2) {
                    return true;
                }
            }
        }
        return false;
    }
    private int GetFactionsInRegion(Region region) {
        List<Faction> factionsInRegion = new List<Faction>();
        for (int i = 0; i < region.settlementsInRegion.Count; i++) {
            BaseSettlement settlement = region.settlementsInRegion[i];
            if (settlement is NPCSettlement && settlement.owner != null && settlement.owner.isMajorNonPlayer && !factionsInRegion.Contains(settlement.owner)) {
                factionsInRegion.Add(settlement.owner);
            }
        }
        return factionsInRegion.Count;
    }
}
