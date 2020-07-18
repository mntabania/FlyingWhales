using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
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
        if (character.currentStructure is Kennel) {
            return false;
        }
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters == null || webbedCharacters.Count <= 2) { //check if there are only 2 or less abducted "Food" at home structure
                if (character.behaviourComponent.currentAbductTarget != null 
                    && (character.behaviourComponent.currentAbductTarget.isDead 
                        || character.behaviourComponent.currentAbductTarget.traitContainer.HasTrait("Restrained"))) {
                    character.behaviourComponent.SetAbductionTarget(null);
                }
            
                //set abduction target if none, and chance met
                if (character.homeStructure != null && character.behaviourComponent.currentAbductTarget == null  && GameUtilities.RollChance(8)) {
                    //check if there are any available animals first
                    List<Character> characterChoices = character.currentRegion.charactersAtLocation
                        .Where(c => c is Animal).ToList();
                    if (characterChoices.Count == 0) {
                        //no available animals, check sleeping characters instead
                        characterChoices = character.currentRegion.charactersAtLocation
                            .Where(c => c.isNormalCharacter && c.traitContainer.HasTrait("Resting")).ToList();
                    }
                    if (characterChoices.Count > 0) {
                        Character chosenCharacter = CollectionUtilities.GetRandomElement(characterChoices);
                        character.behaviourComponent.SetAbductionTarget(chosenCharacter);
                    }
                }

                Character targetCharacter = character.behaviourComponent.currentAbductTarget;
                if (targetCharacter != null) {
                    //create job to abduct target character.
                    return character.jobComponent.TriggerMonsterAbduct(targetCharacter, out producedJob);
                }
            }
        }

        //try to lay an egg
        if (GameUtilities.RollChance(10)) {
            int residentCount = 0;
            int eggCount = 0;
            if (character.homeStructure != null) {
                residentCount = character.homeStructure.residents.Count(x => x.isDead == false);
                eggCount = character.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.SPIDER_EGG).Count;
            } else if (character.territorries.Count > 0) {
                residentCount = character.homeRegion.GetCharactersWithSameTerritory(character)?.Count ?? 0;
                for (int i = 0; i < character.territorries.Count; i++) {
                    HexTile hexTile = character.territorries[i];
                    eggCount += hexTile.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.SPIDER_EGG).Count;
                }
            }
            if (residentCount < 5 && eggCount < 2) {
                return character.jobComponent.TriggerLayEgg(out producedJob);
            }
        }

        if (GameUtilities.RollChance(30)) {
            //Try and eat a webbed character at this spiders home cave
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters != null && webbedCharacters.Count > 0) {
                Character webbedCharacter = CollectionUtilities.GetRandomElement(webbedCharacters);
                return character.jobComponent.TriggerEatAlive(webbedCharacter, out producedJob);
            }    
        }
        
        return character.jobComponent.TriggerRoamAroundTerritory(out producedJob, true);
    }

    private List<Character> GetWebbedCharactersAtHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.GetCharactersThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
        } else if (character.territorries != null && character.territorries.Count > 0) {
            List<Character> characters = null;
            for (int i = 0; i < character.territorries.Count; i++) {
                HexTile territory = character.territorries[i];
                List<Character> validCharacters =
                    territory.GetAllCharactersInsideHexThatMeetCriteria(c => c.traitContainer.HasTrait("Webbed"));
                if (validCharacters != null) {
                    if (characters == null) {
                        characters = new List<Character>();
                    }
                    characters.AddRange(validCharacters);
                }
            }
            return characters;
        }
        return null;
    }
    
    //private void OnPathComplete(Path path, Character character) {
    //    //current abduct path was set to null because path towards target character is already possible, do not process this
    //    if (character.behaviourComponent.currentAbductDigPath == null) { return; } 
        
    //    Vector3 lastPositionInPath = path.vectorPath.Last();
    //    //no path to target tile
    //    //create job to dig wall
    //    LocationGridTile targetTile;
        
    //    LocationGridTile tile = character.currentRegion.innerMap.GetTile(lastPositionInPath);
    //    if (tile.objHere is BlockWall) {
    //        targetTile = tile;
    //    } else {
    //        Vector2 direction = lastPositionInPath - tile.centeredWorldLocation; //character.behaviourComponent.currentAbductTarget.worldPosition - tile.centeredWorldLocation;
    //        if (direction.y > 0) {
    //            //north
    //            targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.North);
    //        } else if (direction.y < 0) {
    //            //south
    //            targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.South);
    //        } else if (direction.x > 0) {
    //            //east
    //            targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.East);
    //        } else {
    //            //west
    //            targetTile = tile.GetNeighbourAtDirection(GridNeighbourDirection.West);
    //        }
    //        if (targetTile != null && targetTile.objHere == null) {
    //            for (int i = 0; i < targetTile.neighbourList.Count; i++) {
    //                LocationGridTile neighbour = targetTile.neighbourList[i];
    //                if (neighbour.objHere is BlockWall) {
    //                    targetTile = neighbour;
    //                    break;
    //                }
    //            }
    //        }
    //    }
        
        
    //    Debug.Log($"No Path found for {character.name} towards {character.behaviourComponent.currentAbductTarget?.name ?? "null"}! Last position in path is {lastPositionInPath.ToString()}. Wall to dig is at {targetTile}");
    //    Assert.IsNotNull(targetTile.objHere, $"Object at {targetTile} is null, but {character.name} wants to dig it.");
        
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DIG_THROUGH, INTERACTION_TYPE.DIG,
    //        targetTile.objHere, character);
    //    character.jobQueue.AddJobInQueue(job);
    //    // character.behaviourComponent.SetDigForAbductionPath(null); //so behaviour can be run again after job has been added
    //}
}
