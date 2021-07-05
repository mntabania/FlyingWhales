using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class DemonRaidBehaviour : CharacterBehaviourComponent {
    public DemonRaidBehaviour() {
        priority = 200;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;
#if DEBUG_LOG
        log += $"\n-Character is raiding";
#endif
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
#if DEBUG_LOG
            log += $"\n-Party is working";
#endif
            if (party.targetDestination.IsAtTargetDestination(character)) {
#if DEBUG_LOG
                log += $"\n-Character is at target destination, do work";
#endif
                DemonRaidPartyQuest quest = party.currentQuest as DemonRaidPartyQuest;
                quest.SetIsSuccessful(true);
                if (quest.target == null) {
                    party.GoBackHomeAndEndQuest();
                    return true;
                }
                BaseSettlement targetSettlement = quest.targetSettlement;
                TileObject target = targetSettlement.GetRandomTileObjectForRaidAttack();
                if (target != null) {
#if DEBUG_LOG
                    log += $"\n-Chosen target is {target.name}";
#endif
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                } else {
#if DEBUG_LOGs
                    log += $"\n-Roam around";
#endif
                    LocationStructure structure = targetSettlement.GetRandomStructure();
                    if (structure != null) {
                        LocationGridTile tile = structure.GetRandomPassableTile();
                        if (tile != null) {
                            hasJob = character.jobComponent.CreateGoToSpecificTileJob(tile, out producedJob);
                        } else {
                            hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        }
                    } else {
                        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    }
                }
            } 
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return hasJob;
    }
}
