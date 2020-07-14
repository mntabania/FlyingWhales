using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class KoboldBehaviour : CharacterBehaviourComponent {
    
    public KoboldBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is Kobold";
        if (UnityEngine.Random.Range(0, 100) < 10) {
            log += $"\nChance to place freezing trap met.";
            // List<HexTile> hexTileChoices = GetTilesNextToActiveSetztlement(character.currentRegion);
            List<HexTile> hexTileChoices = GetValidHexTilesNextToHome(character);
            if (hexTileChoices != null && hexTileChoices.Count > 0) {
                HexTile chosenTile = CollectionUtilities.GetRandomElement(hexTileChoices);
                List<LocationGridTile> locationGridTileChoices =
                    chosenTile.locationGridTiles.Where(x => 
                        x.hasFreezingTrap == false && x.isOccupied == false && x.IsNextToSettlement() == false).ToList();
                if (locationGridTileChoices.Count > 0) {
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(locationGridTileChoices);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP,
                        INTERACTION_TYPE.PLACE_FREEZING_TRAP, targetTile.genericTileObject, character);
                    job.AddOtherData(INTERACTION_TYPE.PLACE_FREEZING_TRAP,  
                        new object[] { new TrapChecker(c => c.race != RACE.KOBOLD) });
                    producedJob = job;
                    log += $"\nCreated job to place trap at {targetTile}.";
                    return true;
                } else {
                    log += $"\nNo valid tiles at {chosenTile} to place trap.";
                    producedJob = null;
                    return false;
                }
            } else {
                log += $"\nNo valid areas to place freezing traps found.";
                producedJob = null;
                return false;
            }
        } else {
            log += $"\nChance to place freezing trap NOT met.";
            List<Character> frozenCharacters = GetFrozenCharactersSurroundingHome(character);
            if (frozenCharacters != null) {
                //check if a character is frozen in any of the neighbouring areas,
                //if there are, then create a job to carry then drop them at this character's home/territory
                Character chosenCharacter = CollectionUtilities.GetRandomElement(frozenCharacters);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP,
                    chosenCharacter, character);
                if (character.homeStructure != null) {
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {character.homeStructure});    
                } else if (character.territorries != null && character.territorries.Count > 0) {
                    HexTile chosenTerritory = CollectionUtilities.GetRandomElement(character.territorries);
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {targetTile.structure, targetTile});
                }
                producedJob = job;
                log += $"\nFrozen character at surrounding area found, will carry {chosenCharacter.name} and drop at home.";
                return true;
            } else {
                log += $"\nNo frozen characters at surrounding area found, checking frozen characters at home.";
                //if there are none, check if there are any characters inside this character's home/territory that is frozen
                List<Character> frozenCharactersAtHome = GetFrozenCharactersInHome(character);
                if (frozenCharactersAtHome != null && frozenCharactersAtHome.Count > 0) {
                    log += $"\nFrozen characters at home found.";
                    //if there are, 8% chance to butcher one, otherwise mock or laugh at one
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(frozenCharactersAtHome);
                    if (GameUtilities.RollChance(8)) {
                        log += $"\nChance to butcher met, will butcher {chosenCharacter.name}.";
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER,
                                            chosenCharacter, character);
                        job.SetCancelOnDeath(false);
                        producedJob = job;
                        return true;
                    } else {
                        log += $"\nChance to butcher NOT met, will roll Mock/Laugh At {chosenCharacter.name} instead.";
                        if (GameUtilities.RollChance(30) && character.marker.inVisionCharacters.Contains(chosenCharacter)) {
                            log += $"\nMock/Laugh triggered.";
                            character.interruptComponent.TriggerInterrupt(
                                GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, chosenCharacter);
                            producedJob = null;
                            return true;    
                        }
                    }
                }
            }
            
            log += $"\nNo jobs related to frozen characters created. Checking food piles at home";
            //if none of the jobs above were created, check for food piles inside this character's home/territory,
            if (GameUtilities.RollChance(15)) {
                List<FoodPile> foodPiles = GetFoodPilesAtHome(character);
                if (foodPiles != null && foodPiles.Count > 0) {
                    //if there are any, create job to eat a random food pile
                    FoodPile chosenPile = CollectionUtilities.GetRandomElement(foodPiles);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT,
                        chosenPile, character);
                    producedJob = job;
                    log += $"\nFood Piles found, will eat {chosenPile}";
                    return true;
                }
            }
            log += $"\nNo food piles found, will roam around territory";
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            
        }
    }
    private List<HexTile> GetValidHexTilesNextToHome(Character character) {
        if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                HexTile homeTile = CollectionUtilities.GetRandomElement(cave.occupiedHexTiles).hexTileOwner;
                return homeTile.AllNeighbours.Where(x => x.region == homeTile.region && x.freezingTraps < 4).ToList();
            } else {
                HexTile homeTile = character.homeStructure.occupiedHexTile.hexTileOwner;
                return homeTile.AllNeighbours.Where(x => x.region == homeTile.region && x.freezingTraps < 4).ToList();    
            }
        } else if (character.territorries != null && character.territorries.Count > 0) {
            HexTile homeTile = CollectionUtilities.GetRandomElement(character.territorries);
            return homeTile.AllNeighbours.Where(x => x.region == homeTile.region && x.freezingTraps < 4).ToList();
        }
        return null;
    }
    private List<Character> GetFrozenCharactersSurroundingHome(Character character) {
        List<Character> characters = null;
        List<HexTile> surroundingAreas = GetAreasSurroundingHome(character);
        for (int i = 0; i < surroundingAreas.Count; i++) {
            HexTile tile = surroundingAreas[i];
            List<Character> charactersAtTile = tile.GetAllCharactersInsideHex();
            if (charactersAtTile != null) {
                for (int j = 0; j < charactersAtTile.Count; j++) {
                    Character characterAtTile = charactersAtTile[j];
                    if (characterAtTile.traitContainer.HasTrait("Frozen") && characterAtTile.race != RACE.KOBOLD && 
                        characterAtTile.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER) == false) {
                        if (characters == null) {
                            characters = new List<Character>();
                        }
                        characters.Add(characterAtTile);
                    }
                }    
            }
            
        }
        return characters;
    }
    private List<Character> GetFrozenCharactersInHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.charactersHere.Where(x => x.traitContainer.HasTrait("Frozen") && x.race != RACE.KOBOLD).ToList();
        } else if (character.territorries != null && character.territorries.Count > 0) {
            List<Character> characters = null;
            for (int i = 0; i < character.territorries.Count; i++) {
                HexTile territory = character.territorries[i];
                List<Character> charactersAtTile = territory.GetAllCharactersInsideHex();
                if (charactersAtTile != null) {
                    for (int j = 0; j < charactersAtTile.Count; j++) {
                        Character characterAtTile = charactersAtTile[j];
                        if (characterAtTile.traitContainer.HasTrait("Frozen") && characterAtTile.race != RACE.KOBOLD) {
                            if (characters == null) {
                                characters = new List<Character>();
                            }
                            characters.Add(characterAtTile);
                        }
                    }
                }
            }
            return characters;
        }
        return null;
    }

    private List<HexTile> GetAreasSurroundingHome(Character character) {
        if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                HexTile homeTile = CollectionUtilities.GetRandomElement(cave.occupiedHexTiles).hexTileOwner;
                return homeTile.AllNeighbours.Where(x => x.region == homeTile.region && x.freezingTraps < 4).ToList();
            } else {
                HexTile homeTile = character.homeStructure.occupiedHexTile.hexTileOwner;
                return homeTile.AllNeighbours.Where(x => x.region == homeTile.region && x.freezingTraps < 4).ToList();    
            }
        } else if (character.territorries != null && character.territorries.Count > 0) {
            List<HexTile> surroundingAreas = new List<HexTile>();
            for (int i = 0; i < character.territorries.Count; i++) {
                HexTile territory = character.territorries[i];
                List<HexTile> validNeighbours =
                    territory.AllNeighbours.Where(x => x.region == territory.region && 
                                                       character.territorries.Contains(x) == false).ToList();
                surroundingAreas.AddRange(validNeighbours);
            }
            return surroundingAreas;
        }
        return null;
    }

    private List<FoodPile> GetFoodPilesAtHome(Character character) {
        if (character.homeStructure != null) {
            return character.homeStructure.GetTileObjectsOfType<FoodPile>();
        } else if (character.territorries != null && character.territorries.Count > 0) {
            List<FoodPile> foodPiles = null;
            for (int i = 0; i < character.territorries.Count; i++) {
                HexTile territory = character.territorries[i];
                List<FoodPile> foodPilesAtTerritory = territory.GetTileObjectsInHexTile<FoodPile>();
                if (foodPilesAtTerritory != null) {
                    if (foodPiles == null) {
                        foodPiles = new List<FoodPile>();
                    }
                    foodPiles.AddRange(foodPilesAtTerritory);
                }
            }
            return foodPiles;
        }
        return null;
    }
}