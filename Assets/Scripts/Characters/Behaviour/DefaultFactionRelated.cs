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
#if DEBUG_LOG
                log += $"\n-{character.name} is factionless, 15% chance to join faction";
#endif
                Faction chosenFaction = character.JoinFactionProcessing();
                if (chosenFaction != null) {
#if DEBUG_LOG
                    log += $"\n-Chosen faction to join: {chosenFaction.name}";
#endif
                } else {
#if DEBUG_LOG
                    log += "\n-No available faction that the character fits the ideology";
#endif
                }
                return true;
            } else {
#if DEBUG_LOG
                log += $"\nDid not meet chance to join faction. Checking if can create faction...";
#endif
                if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableNewFactions) {
                    int villagerFactionCount = FactionManager.Instance.GetActiveVillagerFactionCount();
#if DEBUG_LOG
                    log += $"\nActive villager faction count is {villagerFactionCount.ToString()}";
#endif
                    if (villagerFactionCount < 20) {
#if DEBUG_LOG
                        log += $"\nActive villager factions is less than 20, rolling chances";
#endif
                        int factionsInRegion = GetFactionsInRegion(character.currentRegion);
                        float createChance = factionsInRegion >= 2 ? 2f : 3f;
                        if (character.traitContainer.HasTrait("Inspiring", "Ambitious")) {
                            createChance = factionsInRegion >= 2 ? 0.5f : 15f;
                        }
                        if (GameUtilities.RollChance(createChance)) {
#if DEBUG_LOG
                            log += $"\nChance met, creating new faction";
#endif
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
#if DEBUG_LOG
                log += $"\n-{character.name} is cultist";
#endif
                if (character.faction == null || character.faction.factionType.type != FACTION_TYPE.Demon_Cult) {
#if DEBUG_LOG
                    log += $"\n-Character is not part of a demon cult faction, will try to join one";
#endif
                    int chance = 0;
                    if (HasFactionWithMemberWithUnoccupiedDwelling(FACTION_TYPE.Demon_Cult)) {
#if DEBUG_LOG
                        log += $"\n-Has demon cult faction with a member that has unoccupied dwelling, +3%";
#endif
                        chance += 3;
                    }
                    if (HasFactionWith2Members(FACTION_TYPE.Demon_Cult)) {
#if DEBUG_LOG
                        log += $"\n-Has demon cult faction with 1 or 2 members, +3%";
#endif
                        chance += 3;
                    }
                    if (GameUtilities.RollChance(chance)) {
#if DEBUG_LOG
                        log += $"\n-Will join demon cult";
#endif
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
