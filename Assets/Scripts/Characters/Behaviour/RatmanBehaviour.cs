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
        bool isInHome = character.IsAtHome();
        if (isInHome) {
            if (character.behaviourComponent.PlanSettlementOrFactionWorkActions(out producedJob)) {
                //Ratmen can do work actions
                return true;
            }
        }
        if (GameUtilities.RollChance(0.5f)) {
            int residentCount = character.GetAliveResidentsCountInHome();
            if (residentCount >= 6) {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home_Ratman, null);
            }
        }
        TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
        if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT) {
            //Night time
            int chance = 10;
            if(HasResidentFromSameHomeThatIsNotDeadAndEnslaved(character)) {
                chance -= 7;
            }
            if (HasFoodPileInHomeStorage(character)) {
                chance -= 4;
            }
            if (GameUtilities.RollChance(chance)) {
                if (isInHome) {
                    Character prisoner = GetFirstPrisonerAtHome(character);
                    if (prisoner == null && !HasResidentFromSameHomeThatHasMonsterAbductJob(character)) {
                        character.behaviourComponent.SetAbductionTarget(null);

                        //set abduction target if none, and chance met
                        if (character.behaviourComponent.currentAbductTarget == null && character.currentRegion != null) {
                            List<Character> characterChoices = ObjectPoolManager.Instance.CreateNewCharactersList();
                            for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                                Character characterAtRegion = character.currentRegion.charactersAtLocation[i];
                                if (!characterAtRegion.isDead && characterAtRegion != character
                                    //&& (characterAtRegion.faction?.factionType.type == FACTION_TYPE.Wild_Monsters || characterAtRegion.isNormalCharacter)
                                    && !(characterAtRegion.currentStructure is Kennel)
                                    && !characterAtRegion.traitContainer.HasTrait("Enslaved", "Hibernating")
                                    && (CanProduceFood(characterAtRegion) || CanBeButchered(characterAtRegion))) {
                                    bool isFriendlyWithCharacter = characterAtRegion.faction == character.faction || (characterAtRegion.faction != null && character.faction != null && character.faction.IsFriendlyWith(characterAtRegion.faction));
                                    if (!isFriendlyWithCharacter) {
                                        characterChoices.Add(characterAtRegion);
                                    }
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
                            LocationGridTile targetStructureToDrop = null;
                            if (character.homeStructure != null) {
                                if (!(character.homeStructure is ThePortal)) {
                                    targetStructureToDrop = character.homeStructure.GetRandomPassableTile();
                                }
                            } else if(character.homeSettlement != null && character.homeSettlement.mainStorage != null) {
                                targetStructureToDrop = character.homeSettlement.mainStorage.GetRandomPassableTile();
                            }
                            if (targetStructureToDrop != null) {
                                if (character.jobComponent.TriggerMonsterAbduct(targetCharacter, out producedJob, targetStructureToDrop)) {
                                    character.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        } else {
            //Day time
            if (isInHome) {
                Character prisoner = GetFirstPrisonerAtHome(character);
                if (prisoner != null) {
                    if (GameUtilities.RollChance(30) && prisoner.race == RACE.RATMAN) {
                        return character.jobComponent.TriggerRecruitJob(prisoner, out producedJob);
                    } else if (GameUtilities.RollChance(20) && CanProduceFood(prisoner) && !HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(character)) {
                        return character.jobComponent.TriggerTorture(prisoner, out producedJob);
                    } else if (GameUtilities.RollChance(30) && CanBeButchered(prisoner) && !HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(character) /*&& HasStorage(character)*/ && !HasFoodPileInHomeStorage(character)) {
                        return character.jobComponent.CreateButcherJob(prisoner, JOB_TYPE.MONSTER_BUTCHER, out producedJob);
                    }
                }
            }
        }
        //try to give birth to another ratman
        if (GameUtilities.RollChance(1) && isInHome) {//10
            int residentCount = character.GetAliveResidentsCountInHome();
            if (residentCount < 6) {
                return character.jobComponent.TriggerBirthRatman(out producedJob);
            }
        }
        if (!isInHome) {
            return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
        }
        return character.jobComponent.TriggerRoamAroundTile(out producedJob);
    }
    private bool HasResidentFromSameHomeThatIsNotDeadAndEnslaved(Character character) {
        List<Character> residents;
        bool hasBorrowedList = PopulateResidentsFromSameHome(out residents, character);
        bool decision = true;
        if (residents != null) {
            decision = false;
            for (int i = 0; i < residents.Count; i++) {
                Character r = residents[i];
                if(r != character) {
                    if (!r.isDead && r.traitContainer.HasTrait("Enslaved")) {
                        decision = true;
                        break;
                    }
                }
            }
        }
        if (hasBorrowedList) {
            RuinarchListPool<Character>.Release(residents);
        }
        //If character has no home, this should return true so that the character will not do the action
        return decision;
    }
    private bool HasResidentFromSameHomeThatHasMonsterAbductJob(Character character) {
        List<Character> residents;
        bool hasBorrowedList = PopulateResidentsFromSameHome(out residents, character);
        bool decision = true;
        if (residents != null) {
            decision = false;
            for (int i = 0; i < residents.Count; i++) {
                Character r = residents[i];
                if (r != character) {
                    if (r.jobQueue.HasJob(JOB_TYPE.MONSTER_ABDUCT)) {
                        decision = true;
                        break;
                    }
                }
            }
        }
        if (hasBorrowedList) {
            RuinarchListPool<Character>.Release(residents);
        }
        //If character has no home, this should return true so that the character will not do the action
        return decision;
    }
    private bool HasResidentFromSameHomeThatHasTortureOrMonsterButcherJob(Character character) {
        List<Character> residents;
        bool hasBorrowedList = PopulateResidentsFromSameHome(out residents, character);
        bool decision = true;
        if (residents != null) {
            decision = false;
            for (int i = 0; i < residents.Count; i++) {
                Character r = residents[i];
                if (r != character) {
                    if (r.jobQueue.HasJob(JOB_TYPE.TORTURE, JOB_TYPE.MONSTER_BUTCHER)) {
                        decision = true;
                        break;
                    }
                }
            }
        }
        if (hasBorrowedList) {
            RuinarchListPool<Character>.Release(residents);
        }
        //If character has no home, this should return true so that the character will not do the action
        return decision;
    }

    //Returns true if has borrowed list from the pool
    private bool PopulateResidentsFromSameHome(out List<Character> residents, Character character) {
        bool hasBorrowedList = false;
        residents = null;
        if (character.homeSettlement != null) {
            residents = character.homeSettlement.residents;
        } else if (character.homeStructure != null) {
            residents = character.homeStructure.residents;
        } else if (character.HasTerritory()) {
            hasBorrowedList = true;
            residents = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character resident = CharacterManager.Instance.allCharacters[i];
                if (!resident.isDead && resident.IsTerritory(character.territory)) {
                    residents.Add(resident);
                }
            }
        }
        return hasBorrowedList;
    }
    private Character GetFirstPrisonerAtHome(Character character) {
        if (character.homeSettlement != null) {
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
        } else if (character.homeStructure != null) {
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
        }
        return null;
    }

    private bool CanProduceFood(Character character) {
        if(character.race == RACE.HUMANS || character.race == RACE.ELVES || character is Incubus || character is Succubus || character.race == RACE.NYMPH || character.race == RACE.KOBOLD
            || character.race == RACE.TROLL || character.race == RACE.RATMAN || character.race == RACE.ABOMINATION || character.race == RACE.GOLEM
            || (character.race == RACE.ENT && character is Ent ent && !ent.isTree) || character is GiantSpider) {
            return true;
        }
        return false;
    }
    private bool CanBeButchered(Character character) {
        if ((character is Animal && !(character is Rat)) || character.race == RACE.ELVES || character.race == RACE.HUMANS 
            || character is GiantSpider || character is SmallSpider || character is Wolf) {
            return true;
        }
        return false;
    }
    private bool HasFoodPileInHomeStorage(Character character) {
        LocationStructure storage = null;
        if(character.homeSettlement != null) {
            storage = character.homeSettlement.mainStorage;
        } else if (character.homeStructure != null) {
            storage = character.homeStructure;
        } else if (character.HasTerritory()) {
            return character.territory.tileObjectComponent.HasBuiltFoodPileInArea();
        }
        if(storage != null) {
            return storage.HasTileObjectThatIsBuiltFoodPile();
        }
        return false;
    }
    private bool HasStorage(Character character) {
        LocationStructure storage = null;
        if (character.homeSettlement != null) {
            storage = character.homeSettlement.mainStorage;
        } else if (character.homeStructure != null) {
            storage = character.homeStructure;
        } else if (character.HasTerritory()) {
            return true;
        }
        return storage != null;
    }
}
