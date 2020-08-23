using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RaidBehaviour : CharacterBehaviourComponent {
    public RaidBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is exterminating";
        Party raidParty = character.partyComponent.currentParty;
        if (!raidParty.isWaitTimeOver) {
            log += $"\n-Party is waiting";
            if(raidParty.waitingHexArea != null) {
                log += $"\n-Party has waiting area";
                if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == raidParty.waitingHexArea) {
                        log += $"\n-Character is in waiting area, roam";
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    } else {
                        log += $"\n-Character is not in waiting area, go to it";
                        LocationGridTile targetTile = raidParty.waitingHexArea.GetRandomTile();
                        character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                    }
                }
            } else {
                log += $"\n-Party has no waiting area";
            }
        } else {
            log += $"\n-Party is not waiting";
            if(character.currentStructure.settlementLocation == raidParty.target.currentSettlement) {
                log += $"\n-Character is already in target settlement";
                Character target = character.currentStructure.settlementLocation.GetRandomAliveResidentInsideSettlement();
                if (target != null) {
                    log += $"\n-Chosen target is {target.name}";
                    character.combatComponent.Fight(target, CombatManager.Hostility);
                } else {
                    log += $"\n-Roam around";
                    character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
                log += $"\n-Character is not in target structure, go to it";
                if (raidParty.target is LocationStructure targetStructure) {
                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                    character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        return true;
    }
}
