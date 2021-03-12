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
        log += $"\n-Character is raiding";
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
            log += $"\n-Party is working";
            if (party.targetDestination.IsAtTargetDestination(character)) {
                log += $"\n-Character is at target destination, do work";
                RaidPartyQuest quest = party.currentQuest as RaidPartyQuest;
                if (quest.target == null) {
                    party.GoBackHomeAndEndQuest();
                    return true;
                }
                BaseSettlement targetSettlement = quest.targetSettlement;
                Character target = targetSettlement.GetRandomResidentThatMeetCriteria(resident => character != resident && !resident.isDead && !resident.isBeingSeized && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(targetSettlement) && !resident.traitContainer.HasTrait("Hibernating", "Indestructible"));
                if (target != null) {
                    log += $"\n-Chosen target is {target.name}";
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                } else {
                    log += $"\n-Roam around";
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
