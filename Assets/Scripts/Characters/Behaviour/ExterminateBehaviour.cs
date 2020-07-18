using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class ExterminateBehaviour : CharacterBehaviourComponent {
    public ExterminateBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-Character is exterminating";
        Party exterminateParty = character.partyComponent.currentParty;
        if (!exterminateParty.isWaitTimeOver) {
            log += $"\n-Party is waiting";
            if(exterminateParty.waitingHexArea != null) {
                log += $"\n-Party has waiting area";
                if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                    if (character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == exterminateParty.waitingHexArea) {
                        log += $"\n-Character is in waiting area, roam";
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                    } else {
                        log += $"\n-Character is not in waiting area, go to it";
                        LocationGridTile targetTile = exterminateParty.waitingHexArea.GetRandomTile();
                        character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    }
                }
            } else {
                log += $"\n-Party has no waiting area";
            }
        } else {
            log += $"\n-Party is not waiting";
            if(character.currentStructure == exterminateParty.target) {
                log += $"\n-Character is already in target structure";
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
                if (exterminateParty.target is LocationStructure targetStructure) {
                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                    character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                }
            }
        }
        return true;
    }
}
