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
        if (character.behaviourComponent.currentMiningPath != null) {
            return true; //wait for path to finish.
        }
        LocationGridTile targetTile = character.behaviourComponent.targetMiningTile;
        if (targetTile == null) {
            HexTile nearestCaveTile = GetNearestCaveTile(character);
            if (nearestCaveTile != null) {
                List<LocationGridTile> tileChoices = nearestCaveTile.locationGridTiles
                    .Where(x => x.isOccupied == false && x.structure.structureType == STRUCTURE_TYPE.CAVE).ToList();
                targetTile = CollectionUtilities.GetRandomElement(tileChoices);
            } else {
                Cave cave = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CAVE) as Cave;
                Assert.IsNotNull(cave, $"Cave in mine behaviour of {character} is null");
                targetTile = CollectionUtilities.GetRandomElement(cave.unoccupiedTiles);
            }
            character.behaviourComponent.SetTargetMiningTile(targetTile);
        }

        if (character.movementComponent.HasPathTo(character.behaviourComponent.targetMiningTile)) {
            character.behaviourComponent.SetCurrentMiningPath(null);
            Debug.Log($"Has Path for {character.name} towards {character.behaviourComponent.targetMiningTile}!");
            //create job to mine target tile.
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE, INTERACTION_TYPE.MINE,
                character.behaviourComponent.targetMiningTile.genericTileObject, character);
            producedJob = job;
        } else {
            ABPath p = ABPath.Construct(character.worldPosition, targetTile.centeredWorldLocation, (path) => OnPathComplete(path, character));
            AstarPath.StartPath(p);
            character.behaviourComponent.SetCurrentMiningPath(p);    
        }
        return true;
    }
    private void OnPathComplete(Path path, Character character) {
        //current mining path was set to null because path towards mining target is already possible, do not process this
        if (character.behaviourComponent.currentMiningPath == null) { return; } 
        
        Vector3 lastPositionInPath = path.vectorPath.Last();
        //no path to target tile
        //create job to dig wall
        LocationGridTile targetTile;
        
        LocationGridTile tile = character.currentRegion.innerMap.GetTile(lastPositionInPath);
        if (tile.objHere is BlockWall) {
            targetTile = tile;
        } else {
            Vector2 direction = character.behaviourComponent.targetMiningTile.centeredWorldLocation - tile.centeredWorldLocation;
            if (direction.y > 0) {
                //north
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.North);
            } else if (direction.y < 0) {
                //south
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.South);
            } else if (direction.x > 0) {
                //east
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.East);
            } else {
                //west
                targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.West);
            }
            if (targetTile != null && targetTile.objHere == null) {
                for (int i = 0; i < targetTile.neighbourList.Count; i++) {
                    LocationGridTile neighbour = targetTile.neighbourList[i];
                    if (neighbour.objHere is BlockWall) {
                        targetTile = neighbour;
                        break;
                    }
                }
            }
        }
        
        
        Debug.Log($"No Path found for {character.name} towards {character.behaviourComponent.targetMiningTile}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
        Assert.IsNotNull(targetTile.objHere, $"Object at {targetTile} is null, but {character.name} wants to dig it.");
        
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG,
            targetTile.objHere, character);
        character.jobQueue.AddJobInQueue(job);
        character.behaviourComponent.SetCurrentMiningPath(null); //so behaviour can be run again after job has been added
    }

    private HexTile GetNearestCaveTile(Character character) {
        if (character.gridTileLocation != null && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            HexTile originTile = character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
            HexTile nearestTile = null;
            float nearestDist = 9999f;
            for (int i = 0; i < character.currentRegion.tiles.Count; i++) {
                HexTile tile = character.currentRegion.tiles[i];
                if (tile.elevationType == ELEVATION.MOUNTAIN) {
                    float distance = Vector2.Distance(originTile.transform.position, tile.transform.position);
                    if (nearestTile == null || distance < nearestDist) {
                        nearestDist = distance;
                        nearestTile = tile;
                    }    
                }
            }
            return nearestTile;
        }
        return null;
    }
}
