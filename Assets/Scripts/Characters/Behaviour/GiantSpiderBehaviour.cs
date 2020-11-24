﻿using System;
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
        //between 12am to 3am
        if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) >= 0 && 
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) <= 3) {
            List<Character> webbedCharacters = GetWebbedCharactersAtHome(character);
            if (webbedCharacters == null || webbedCharacters.Count <= 2) { //check if there are only 2 or less abducted "Food" at home structure
                if (character.behaviourComponent.currentAbductTarget != null 
                    && (character.behaviourComponent.currentAbductTarget.isDead 
                        || character.behaviourComponent.currentAbductTarget.traitContainer.HasTrait("Restrained"))) {
                    character.behaviourComponent.SetAbductionTarget(null);
                }
            
                //set abduction target if none, and chance met
                if (character.homeStructure != null && character.behaviourComponent.currentAbductTarget == null  && GameUtilities.RollChance(2)) {
                    List<Character> characterChoices = ObjectPoolManager.Instance.CreateNewCharactersList();
                    for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                        Character c = character.currentRegion.charactersAtLocation[i];
                        if (c != character && !c.isDead && (c is Animal || (c.isNormalCharacter && c.traitContainer.HasTrait("Resting"))) && c.currentStructure is Kennel == false) {
                            characterChoices.Add(c);
                        }
                    }
                    if (characterChoices.Count > 0) {
                        Character chosenCharacter = CollectionUtilities.GetRandomElement(characterChoices);
                        character.behaviourComponent.SetAbductionTarget(chosenCharacter);
                    }
                    ObjectPoolManager.Instance.ReturnCharactersListToPool(characterChoices);
                }

                Character targetCharacter = character.behaviourComponent.currentAbductTarget;
                if (targetCharacter != null) {
                    //create job to abduct target character.
                    return character.jobComponent.TriggerMonsterAbduct(targetCharacter, out producedJob);
                }
            }
        }

        //try to lay an egg
        if (GameUtilities.RollChance(2) && (character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory())) {//10
            int residentCount = 0;
            int eggCount = 0;
            if (character.homeStructure != null) {
                residentCount = character.homeStructure.residents.Count(x => x.isDead == false);
                eggCount = character.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.SPIDER_EGG).Count;
            } else if (character.HasTerritory()) {
                residentCount = character.homeRegion.GetCountOfCharacterWithSameTerritory(character);
                eggCount += character.territory.GetTileObjectsInHexTile(TILE_OBJECT_TYPE.SPIDER_EGG).Count;
            }
            if (residentCount < 4 && eggCount < 2) {
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
        } else if (character.HasTerritory()) {
            List<Character> characters = character.territory.GetAllCharactersInsideHexThatMeetCriteria<Character>(c => c.traitContainer.HasTrait("Webbed"));
            return characters;
        }
        return null;
    }
   
}
