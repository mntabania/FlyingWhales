using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UtilityScripts;

public class DefendBehaviour : CharacterBehaviourComponent {
    public DefendBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.IsAtHome()) {
            //List<Character> choices = GetTargetChoices(character.territories, character);
            //if (choices != null) {
            //    Character chosenTarget = CollectionUtilities.GetRandomElement(choices);
            //    character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
            //    producedJob = null;
            //    return true;
            //}
            Character hostile = GetFirstHostileIntruder(character);
            if(hostile != null) {
                character.combatComponent.Fight(hostile, CombatManager.Hostility);
                producedJob = null;
                return true;
            } else {
                //Roam around tile
                return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            }
        } else {
            //character is not at home, go back.
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
    }
    private Character GetFirstHostileIntruder(Character actor) {
        if(actor.homeStructure != null) {
            for (int i = 0; i < actor.homeStructure.charactersHere.Count; i++) {
                Character target = actor.homeStructure.charactersHere[i];
                if(actor != target && actor.IsHostileWith(target) && !target.isDead && !target.isAlliedWithPlayer 
                    && target.marker && target.marker.isMainVisualActive && actor.movementComponent.HasPathTo(target.gridTileLocation)
                    && !target.isInLimbo && !target.isBeingSeized && target.carryComponent.IsNotBeingCarried()) {
                    return target;
                }
            }
        } else {
            Area area = actor.areaLocation;
            if(area != null) {
                Character chosenTarget = area.locationCharacterTracker.GetFirstCharacterInsideHexThatIsAliveHostileNotAlliedWithPlayerThatHasPathTo(actor);
                if(chosenTarget != null) {
                    return chosenTarget;
                }
            }
        }
        return null;
    }
    
    // public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
    //     producedJob = null;
    //     log += $"\n-{character.name} will defend";
    //     HexTile chosenHex = character.behaviourComponent.assignedTargetHex;
    //     if (chosenHex != null) {
    //         Character chosenTarget = chosenHex.GetFirstCharacterThatIsNotDeadInsideHexThatIsHostileWith(character);
    //         if(chosenTarget != null) {
    //             character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
    //         } else {
    //             LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenHex.borderTiles);
    //             character.jobComponent.TriggerRoamAroundTile(out producedJob, chosenTile);
    //         }
    //         return true;
    //     }
    //     return false;
    //     //if (character.gridTileLocation.hexTileOwner && character.gridTileLocation.hexTileOwner.settlementOnTile == character.behaviourComponent.assignedTargetSettlement) {
    //     //    log += "\n-Already in the target npcSettlement";
    //     //    TileObject targetTileObject = null;
    //     //    Assert.IsTrue(character.gridTileLocation.hexTileOwner.settlementOnTile is NPCSettlement, $"{character.name} is trying to raid a settlement that is not an NPC Settlement");
    //     //    NPCSettlement npcSettlement = character.gridTileLocation.hexTileOwner.settlementOnTile as NPCSettlement;
    //     //    for (int i = 0; i < npcSettlement.mainStorage.pointsOfInterest.Count; i++) {
    //     //        IPointOfInterest poi = npcSettlement.mainStorage.pointsOfInterest.ElementAt(i);
    //     //        if (poi is Artifact || poi is ResourcePile) {
    //     //            targetTileObject = poi as TileObject;
    //     //            break;
    //     //        }
    //     //    }
    //     //    if (targetTileObject != null) {
    //     //        log += "\n-Has artifact or resource pile in main storage, 50% to destroy tile object";
    //     //        int roll = UnityEngine.Random.Range(0, 100);
    //     //        log += $"\n-Roll: {roll}";
    //     //        if (roll < 50) {
    //     //            log += "\n-Destroying a random tile object";
    //     //            if (targetTileObject is Artifact) {
    //     //                log += "\n-Tile object is an artifact, will carry to portal instead";
    //     //                character.jobComponent.CreateTakeArtifactJob(targetTileObject, PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure);
    //     //            } else {
    //     //                log += "\n-Tile object is a resource pile, will destroy 100 amount";
    //     //                character.jobComponent.CreateDestroyResourceAmountJob(targetTileObject as ResourcePile, 100);
    //     //            }
    //     //        } else {
    //     //            log += "\n-Roam";
    //     //            character.jobComponent.TriggerRoamAroundTile();
    //     //        }
    //     //    } else {
    //     //        log += "\n-No artifact or resource pile is main storage";
    //     //        log += "\n-Roam";
    //     //        character.jobComponent.TriggerRoamAroundTile();
    //     //    }
    //     //} else {
    //     //    log += "\n-Is not in the target npcSettlement";
    //     //    log += "\n-Roam there";
    //     //    HexTile targetHex = character.behaviourComponent.assignedTargetSettlement.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.assignedTargetSettlement.tiles.Count)];
    //     //    LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
    //     //    character.jobComponent.TriggerRoamAroundTile(targetTile);
    //
    //     //}
    //     //return true;
    // }
}