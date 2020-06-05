using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class GiantSpiderBehaviour : CharacterBehaviourComponent {

    public GiantSpiderBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            if (character.behaviourComponent.currentAbductTarget != null && character.behaviourComponent.currentAbductTarget.isDead) {
                character.behaviourComponent.SetAbductionTarget(null);
            }
            
            //set abduction target if none, and chance met
            if (character.homeStructure != null && character.behaviourComponent.currentAbductTarget == null  && Random.Range(0, 100) < 20) {
                List<Character> characterChoices = character.currentRegion.charactersAtLocation
                    .Where(c => c.isNormalCharacter && c.canMove).ToList();
                if (characterChoices.Count > 0) {
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(characterChoices);
                    character.behaviourComponent.SetAbductionTarget(chosenCharacter);
                }
            }

            Character targetCharacter = character.behaviourComponent.currentAbductTarget;
            if (targetCharacter != null) {
                //try to go to abduct target
                if (PathfindingManager.Instance.HasPath(character.gridTileLocation, targetCharacter.gridTileLocation)) {
                    character.behaviourComponent.SetDigForAbductionPath(null);
                    Debug.Log($"Has Path for {character.name} towards {targetCharacter.name}!");
                    //create job to abduct target character.
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT,
                        INTERACTION_TYPE.DROP, targetCharacter, character);
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {character.homeStructure});
                    producedJob = job;
                } else {
                    //if not path towards target, compute path to nearest block wall, then do dig action
                    ABPath p = ABPath.Construct(character.worldPosition, targetCharacter.worldPosition, (path) => OnPathComplete(path, character));
                    AstarPath.StartPath(p);
                    character.behaviourComponent.SetDigForAbductionPath(p);    
                }    
                return true;
            }
            
            
        }
        return false;
    }
    
    private void OnPathComplete(Path path, Character character) {
        //current abduct path was set to null because path towards target character is already possible, do not process this
        if (character.behaviourComponent.currentAbductDigPath == null) { return; } 
        
        Vector3 lastPositionInPath = path.vectorPath.Last();
        //no path to target tile
        //create job to dig wall
        LocationGridTile targetTile;
        
        LocationGridTile tile = character.currentRegion.innerMap.GetTile(lastPositionInPath);
        if (tile.objHere is BlockWall) {
            targetTile = tile;
        } else {
            Vector2 direction = character.behaviourComponent.currentAbductTarget.worldPosition - tile.centeredWorldLocation;
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
        }
        
        
        Debug.Log($"No Path found for {character.name} towards {character.behaviourComponent.targetMiningTile}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
        Assert.IsNotNull(targetTile.objHere, $"Object at {targetTile} is null, but {character.name} wants to dig it.");
        
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG,
            targetTile.objHere, character);
        character.jobQueue.AddJobInQueue(job);
        // character.behaviourComponent.SetDigForAbductionPath(null); //so behaviour can be run again after job has been added
    }
}
