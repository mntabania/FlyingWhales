using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
using Random = UnityEngine.Random;

public class RatmanBehaviour : CharacterBehaviourComponent {

    public RatmanBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        character.combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        bool isInHome = character.IsInHomeSettlement() || character.isAtHomeStructure || character.IsInTerritory();
        if (isInHome) {
            if (character.behaviourComponent.PlanWorkActions(out producedJob)) {
                //Ratmen can do work actions
                return true;
            }
        }
        TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
        if (currentTime == TIME_IN_WORDS.LATE_NIGHT || currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            //Night time
            //if (GameUtilities.RollChance(20)) {
                if(isInHome) {
                    Character prisoner = GetFirstPrisonerAtHome(character);
                    if (prisoner == null && !HasJobTypeFromSameHome(character, JOB_TYPE.MONSTER_ABDUCT)) {
                    character.behaviourComponent.SetAbductionTarget(null);

                    //set abduction target if none, and chance met
                    if (character.behaviourComponent.currentAbductTarget == null && character.currentRegion != null) {
                            List<Character> characterChoices = ObjectPoolManager.Instance.CreateNewCharactersList();
                            for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                                Character characterAtRegion = character.currentRegion.charactersAtLocation[i];
                                if (!characterAtRegion.isDead && characterAtRegion != character
                                    && (characterAtRegion.faction?.factionType.type == FACTION_TYPE.Wild_Monsters || (characterAtRegion.isNormalCharacter && characterAtRegion.traitContainer.HasTrait("Resting")))
                                    && !(characterAtRegion.currentStructure is Kennel)
                                    && !characterAtRegion.traitContainer.HasTrait("Enslaved")
                                    && characterAtRegion.faction != character.faction) {
                                    characterChoices.Add(characterAtRegion);
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
                            if(character.jobComponent.TriggerMonsterAbduct(targetCharacter, out producedJob, cannotBePushedBack: false)) {
                                character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
                                return true;
                            }
                        }
                    }
                }
            //}
        } else {
            //Day time
            if (GameUtilities.RollChance(20)) {
                if (isInHome) {
                    Character prisoner = GetFirstPrisonerAtHome(character);
                    if (prisoner != null && !HasJobTypeFromSameHome(character, JOB_TYPE.TORTURE)) {
                        return character.jobComponent.TriggerTorture(prisoner, out producedJob);
                    }
                }
            }
        }
        //try to give birth to another ratman
        if (GameUtilities.RollChance(2) && isInHome) {//10
            int residentCount = 0;
            if (character.homeSettlement != null) {
                residentCount = character.homeSettlement.residents.Count(x => x.isDead == false);
            } else if (character.homeStructure != null) {
                residentCount = character.homeStructure.residents.Count(x => x.isDead == false);
            } else if (character.HasTerritory()) {
                residentCount = character.homeRegion.GetCountOfCharacterWithSameTerritory(character);
            }
            if (residentCount < 8) {
                return character.jobComponent.TriggerBirthRatman(out producedJob);
            }
        }
        if (!isInHome) {
            return character.jobComponent.TriggerReturnTerritory(out producedJob);
        }
        return character.jobComponent.TriggerRoamAroundTile(out producedJob);
    }
    private bool HasJobTypeFromSameHome(Character character, JOB_TYPE jobType) {
        List<Character> residents = null;
        if(character.homeSettlement != null) {
            residents = character.homeSettlement.residents;
        } else if (character.homeStructure != null) {
            residents = character.homeStructure.residents;
        }
        if(residents != null) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if(resident != character) {
                    if (resident.jobQueue.HasJob(jobType)) {
                        return true;
                    }
                }
            }
            return false;
        }
        //If character has no home, this should return true so that the character will not do the action
        return true;
    }
    private Character GetFirstPrisonerAtHome(Character character) {
        if (character.homeStructure != null) {
            for (int i = 0; i < character.homeStructure.charactersHere.Count; i++) {
                Character potentialPrisoner = character.homeStructure.charactersHere[i];
                if (!potentialPrisoner.isDead) {
                    if (potentialPrisoner.traitContainer.HasTrait("Prisoner")) {
                        Prisoner prisoner = potentialPrisoner.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                        if (prisoner.IsConsideredPrisonerOf(character)) {
                            return potentialPrisoner;
                        }
                    }
                }
            }
        } else if (character.homeSettlement != null) {
            for (int i = 0; i < character.homeSettlement.region.charactersAtLocation.Count; i++) {
                Character potentialPrisoner = character.homeSettlement.region.charactersAtLocation[i];
                if (!potentialPrisoner.isDead) {
                    if(potentialPrisoner.gridTileLocation != null && potentialPrisoner.gridTileLocation.IsPartOfSettlement(character.homeSettlement)) {
                        if (potentialPrisoner.traitContainer.HasTrait("Prisoner")) {
                            Prisoner prisoner = potentialPrisoner.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                            if (prisoner.IsConsideredPrisonerOf(character)) {
                                return potentialPrisoner;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
   
}
