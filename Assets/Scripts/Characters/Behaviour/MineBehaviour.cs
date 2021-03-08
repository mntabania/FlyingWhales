using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class MineBehaviour : CharacterBehaviourComponent {
    
    public MineBehaviour() {
        priority = 440;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        NPCSettlement homeSettlement = character.homeSettlement;
        if (homeSettlement != null) {
            List<LocationStructure> mineShacks = homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE_SHACK);
            LocationGridTile targetTile = null;
            if(mineShacks != null && mineShacks.Count > 0) {
                for (int i = 0; i < mineShacks.Count; i++) {
                    MineShack mineShack = mineShacks[i] as MineShack;
                    if(mineShack != null && mineShack.connectedCave != null) {
                        targetTile = mineShack.connectedCave.GetRandomPassableTileThatMeetCriteria(t => !t.isOccupied);
                        if (targetTile != null) {
                            break;
                        }
                    }
                }
            }
            if(targetTile != null) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE, INTERACTION_TYPE.MINE, targetTile.tileObjectComponent.genericTileObject, character);
                job.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                producedJob = job;
                return true;
            }
        }

        //LocationGridTile targetTile = character.behaviourComponent.targetMiningTile;
        //if (targetTile == null) {
        //    HexTile nearestCaveTile = GetNearestCaveTile(character);
        //    if (nearestCaveTile != null && nearestCaveTile.locationGridTiles != null && nearestCaveTile.locationGridTiles.Count > 0) {
        //        List<LocationGridTile> tileChoices = nearestCaveTile.locationGridTiles.Where(x => x.isOccupied == false && x.structure.structureType == STRUCTURE_TYPE.CAVE).ToList();
        //        targetTile = CollectionUtilities.GetRandomElement(tileChoices);
        //    } else {
        //        if(character.currentRegion != null) {
        //            Cave cave = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CAVE) as Cave;
        //            //Assert.IsNotNull(cave, $"Cave in mine behaviour of {character} is null");
        //            if(cave != null && cave.unoccupiedTiles != null && cave.unoccupiedTiles.Count > 0) {
        //                targetTile = CollectionUtilities.GetRandomElement(cave.unoccupiedTiles);
        //            }
        //        }
        //    }
        //    character.behaviourComponent.SetTargetMiningTile(targetTile);
        //}
        //if (character.behaviourComponent.targetMiningTile != null) {
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE, INTERACTION_TYPE.MINE, character.behaviourComponent.targetMiningTile.tileObjectComponent.genericTileObject, character);
        //    job.SetDoNotRecalculate(true);
        //    job.SetCannotBePushedBack(true);
        //    producedJob = job;
        //    return true;    
        //}
        return false;
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.movementComponent.SetEnableDigging(true);
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.movementComponent.SetEnableDigging(false);
    }
    // private void OnPathComplete(Path path, Character character) {
    //     //current mining path was set to null because path towards mining target is already possible, do not process this
    //     if (character.behaviourComponent.currentMiningPath == null) { return; } 
    //     
    //     Vector3 lastPositionInPath = path.vectorPath.Last();
    //     //no path to target tile
    //     //create job to dig wall
    //     LocationGridTile targetTile;
    //     
    //     LocationGridTile tile = character.currentRegion.innerMap.GetTile(lastPositionInPath);
    //     if (tile.tileObjectComponent.objHere is BlockWall) {
    //         targetTile = tile;
    //     } else {
    //         Vector2 direction = character.behaviourComponent.targetMiningTile.centeredWorldLocation - tile.centeredWorldLocation;
    //         if (direction.y > 0) {
    //             //north
    //             targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.North);
    //         } else if (direction.y < 0) {
    //             //south
    //             targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.South);
    //         } else if (direction.x > 0) {
    //             //east
    //             targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.East);
    //         } else {
    //             //west
    //             targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.West);
    //         }
    //         if (targetTile != null && targetTile.tileObjectComponent.objHere == null) {
    //             for (int i = 0; i < targetTile.neighbourList.Count; i++) {
    //                 LocationGridTile neighbour = targetTile.neighbourList[i];
    //                 if (neighbour.tileObjectComponent.objHere is BlockWall) {
    //                     targetTile = neighbour;
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //     
    //     
    //     Debug.Log($"No Path found for {character.name} towards {character.behaviourComponent.targetMiningTile}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
    //     Assert.IsNotNull(targetTile.tileObjectComponent.objHere, $"Object at {targetTile} is null, but {character.name} wants to dig it.");
    //     
    //     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG,
    //         targetTile.tileObjectComponent.objHere, character);
    //     character.jobQueue.AddJobInQueue(job);
    //     character.behaviourComponent.SetCurrentMiningPath(null); //so behaviour can be run again after job has been added
    // }

    private Area GetNearestCaveArea(Character character) {
        if (character.gridTileLocation != null) {
            Area originArea = character.areaLocation;
            Area nearestArea = null;
            float nearestDist = 0f;
            for (int i = 0; i < character.currentRegion.areas.Count; i++) {
                Area area = character.currentRegion.areas[i];
                if (area.elevationType == ELEVATION.MOUNTAIN) {
                    LocationStructure structure = area.structureComponent.GetMostImportantStructureOnTile();
                    Assert.IsTrue(structure is Cave, $"Most important Structure at {area} is not a cave");
                    if (character.movementComponent.structuresToAvoid.Contains(structure)) {
                        continue; //skip
                    }
                    float distance = Vector2.Distance(originArea.gridTileComponent.centerGridTile.centeredWorldLocation, area.gridTileComponent.centerGridTile.centeredWorldLocation);
                    if (nearestArea == null || distance < nearestDist) {
                        nearestDist = distance;
                        nearestArea = area;
                    }    
                }
            }
            return nearestArea;
        }
        return null;
    }
}
