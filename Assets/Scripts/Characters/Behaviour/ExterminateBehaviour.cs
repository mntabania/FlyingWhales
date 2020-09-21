﻿using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class ExterminateBehaviour : CharacterBehaviourComponent {
    public ExterminateBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is exterminating";
        Party party = character.partyComponent.currentParty;
        if (party.isActive && party.partyState == PARTY_STATE.Working) {
            log += $"\n-Party is working";
            if (party.targetDestination.IsAtTargetDestination(character)) {
                log += $"\n-Character is at target destination, do work";
                Character target = GetRandomAliveResidentInsideSettlementThatIsHostileWith(character, character.currentStructure.settlementLocation);
                if (target != null) {
                    log += $"\n-Chosen target is {target.name}";
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                    return true;
                } else {
                    log += $"\n-End Exterminate";
                    party.GoBackHomeAndEndQuest();
                    return true;
                    //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } 
            //else {
            //    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
            //    return character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
            //}
        }

        //if (!party.isWaitTimeOver) {
        //    log += $"\n-Party is waiting";
        //    if(party.waitingHexArea != null) {
        //        log += $"\n-Party has waiting area";
        //        if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
        //            if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == party.waitingHexArea) {
        //                log += $"\n-Character is in waiting area, roam";
        //                character.jobComponent.TriggerRoamAroundTile(out producedJob);
        //            } else {
        //                log += $"\n-Character is not in waiting area, go to it";
        //                LocationGridTile targetTile = party.waitingHexArea.GetRandomTile();
        //                character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //            }
        //        }
        //    } else {
        //        log += $"\n-Party has no waiting area";
        //    }
        //} else {
        //    log += $"\n-Party is not waiting";
        //    if(character.currentStructure == party.target) {
        //        log += $"\n-Character is already in target structure";
        //        Character target = GetRandomAliveResidentInsideSettlementThatIsHostileWith(character, character.currentStructure.settlementLocation);
        //        if (target != null) {
        //            log += $"\n-Chosen target is {target.name}";
        //            character.combatComponent.Fight(target, CombatManager.Hostility);
        //        } else {
        //            log += $"\n-End Exterminate";
        //            party.GoBackHomeAndEndQuest();
        //            return true;
        //            //character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        //        }
        //    } else {
        //        log += $"\n-Character is not in target structure, go to it";
        //        if (party.target is LocationStructure targetStructure) {
        //            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
        //            character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
        //        }
        //    }
        //}
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return false;
    }

    private Character GetRandomAliveResidentInsideSettlementThatIsHostileWith(Character character, BaseSettlement settlement) {
        List<Character> choices = null;
        for (int i = 0; i < settlement.residents.Count; i++) {
            Character resident = settlement.residents[i];
            if (character != resident 
                && !resident.isDead
                && resident.gridTileLocation != null
                && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                && resident.gridTileLocation.IsPartOfSettlement(settlement)
                && (resident.faction == null || character.faction == null || character.faction.IsHostileWith(resident.faction))) {
                if (choices == null) { choices = new List<Character>(); }
                choices.Add(resident);
            }
        }
        if (choices != null && choices.Count > 0) {
            return choices[UnityEngine.Random.Range(0, choices.Count)];
        }
        return null;
    }
}
