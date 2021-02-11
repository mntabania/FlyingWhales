using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Profiling;
using UtilityScripts;

public class KoboldBehaviour : BaseMonsterBehaviour {
    
    public KoboldBehaviour() {
        priority = 9;
    }
    
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n{character.name} is Kobold";
        if (UnityEngine.Random.Range(0, 100) < 10) {
            log += $"\nChance to place freezing trap met.";
            Profiler.BeginSample($"Kobold Place Freezing Trap");
            List<Area> areaChoices = ObjectPoolManager.Instance.CreateNewAreaList();
            PopulateValidHexTilesNextToHome(areaChoices, character);
            if (areaChoices.Count > 0) {
                Area chosenArea = CollectionUtilities.GetRandomElement(areaChoices);
                ObjectPoolManager.Instance.ReturnAreaListToPool(areaChoices);
                LocationGridTile targetTile = chosenArea.gridTileComponent.GetRandomUnoccupiedNoFreezingTrapNotNextToSettlementTile();
                //List<LocationGridTile> locationGridTileChoices = chosenArea.gridTileComponent.gridTiles.Where(x => 
                //        x.hasFreezingTrap == false && x.isOccupied == false && x.IsNextToSettlement() == false).ToList();
                if (targetTile != null) {
                    //LocationGridTile targetTile = CollectionUtilities.GetRandomElement(locationGridTileChoices);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, INTERACTION_TYPE.PLACE_FREEZING_TRAP, targetTile.genericTileObject, character);
                    producedJob = job;
                    log += $"\nCreated job to place trap at {targetTile}.";
                    Profiler.EndSample();
                    return true;
                } else {
                    log += $"\nNo valid tiles at {chosenArea} to place trap.";
                    producedJob = null;
                    Profiler.EndSample();
                    return false;
                }
            } else {
                ObjectPoolManager.Instance.ReturnAreaListToPool(areaChoices);
                log += $"\nNo valid areas to place freezing traps found.";
                producedJob = null;
                Profiler.EndSample();
                return false;
            }
        } else {
            log += $"\nChance to place freezing trap NOT met.";
            Profiler.BeginSample($"Kobold Get Frozen Characters Surrounding Home");
            List<Character> frozenCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
            PopulateFrozenCharactersSurroundingHome(frozenCharacters, character);
            Profiler.EndSample();
            if (frozenCharacters.Count > 0) {
                //check if a character is frozen in any of the neighbouring areas,
                //if there are, then create a job to carry then drop them at this character's home/territory
                Profiler.BeginSample($"Kobold Capture Frozen Characters");
                Character chosenCharacter = CollectionUtilities.GetRandomElement(frozenCharacters);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP, chosenCharacter, character);
                if (character.homeSettlement?.mainStorage != null) {
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {character.homeSettlement.mainStorage});
                } else if (character.homeStructure != null) {
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {character.homeStructure});    
                } else if (character.HasTerritory()) {
                    Area chosenTerritory = character.territory;
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(chosenTerritory.gridTileComponent.gridTiles);
                    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {targetTile.structure, targetTile});
                }
                ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
                producedJob = job;
                log += $"\nFrozen character at surrounding area found, will carry {chosenCharacter.name} and drop at home.";
                Profiler.EndSample();
                return true;
            } else {
                log += $"\nNo frozen characters at surrounding area found, checking frozen characters at home.";
                //if there are none, check if there are any characters inside this character's home/territory that is frozen
                Profiler.BeginSample($"Kobold Populate Frozen Characters in Home");
                PopulateFrozenCharactersInHome(frozenCharacters, character);
                Profiler.EndSample();
                if (frozenCharacters.Count > 0) {
                    log += $"\nFrozen characters at home found.";
                    //if there are, 8% chance to butcher one, otherwise mock or laugh at one
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(frozenCharacters);
                    if (GameUtilities.RollChance(8)) {
                        log += $"\nChance to butcher met, will butcher {chosenCharacter.name}.";
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, chosenCharacter, character);
                        job.SetCancelOnDeath(false);
                        ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
                        producedJob = job;
                        return true;
                    } else {
                        log += $"\nChance to butcher NOT met, will roll Mock/Laugh At {chosenCharacter.name} instead.";
                        if (GameUtilities.RollChance(30) && character.marker.IsPOIInVision(chosenCharacter)) {
                            log += $"\nMock/Laugh triggered.";
                            character.interruptComponent.TriggerInterrupt(GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, chosenCharacter);
                            ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
                            producedJob = null;
                            return true;    
                        }
                    }
                }
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);

            log += $"\nNo jobs related to frozen characters created. Checking food piles at home";
            //if none of the jobs above were created, check for food piles inside this character's home/territory,
            if (GameUtilities.RollChance(15)) {
                Profiler.BeginSample($"Kobold Get Food Piles at Home");
                List<FoodPile> foodPiles = GetFoodPilesAtHome(character);
                Profiler.EndSample();
                if (foodPiles != null && foodPiles.Count > 0) {
                    Profiler.BeginSample($"Kobold Monster Eat Job");
                    //if there are any, create job to eat a random food pile
                    FoodPile chosenPile = CollectionUtilities.GetRandomElement(foodPiles);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, chosenPile, character);
                    producedJob = job;
                    log += $"\nFood Piles found, will eat {chosenPile}";
                    Profiler.EndSample();
                    return true;
                }
            }
            log += $"\nNo food piles found, will roam around territory";
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            
        }
    }
    private void PopulateValidHexTilesNextToHome(List<Area> areas, Character character) {
        Area homeArea = null;
        if (character.homeSettlement != null) {
            character.homeSettlement.PopulateSurroundingAreasInSameRegionWithLessThanNumOfFreezingTraps(areas, character.homeRegion, 4);
        } else if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                homeArea = CollectionUtilities.GetRandomElement(cave.occupiedAreas);
                //return homeArea.neighbourComponent.neighbours.Where(x => x.region == homeArea.region && x.freezingTraps < 4).ToList();
            } else {
                homeArea = character.homeStructure.occupiedArea;
                //return homeArea.neighbourComponent.neighbours.Where(x => x.region == homeArea.region && x.freezingTraps < 4).ToList();    
            }
        } else if (character.HasTerritory()) {
            homeArea = character.territory;
            //return homeArea.AllNeighbours.Where(x => x.region == homeArea.region && x.freezingTraps < 4).ToList();
        }
        if (homeArea != null) {
            for (int i = 0; i < homeArea.neighbourComponent.neighbours.Count; i++) {
                Area neighbour = homeArea.neighbourComponent.neighbours[i];
                if (neighbour.region == homeArea.region && neighbour.freezingTraps < 4) {
                    areas.Add(neighbour);
                }
            }
        }
        //return null;
    }
    private void PopulateFrozenCharactersSurroundingHome(List<Character> p_characterList, Character character) {
        List<Area> surroundingAreas = ObjectPoolManager.Instance.CreateNewAreaList();
        PopulateAreasSurroundingHome(surroundingAreas, character);
        //if (surroundingAreas != null) {
            for (int i = 0; i < surroundingAreas.Count; i++) {
                Area area = surroundingAreas[i];
                area.locationCharacterTracker.PopulateCharacterListInsideHexThatMeetCriteria(p_characterList, c => c.traitContainer.HasTrait("Frozen") && c.race != RACE.KOBOLD &&
                                                                                                                  c.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER) == false);
            }
        //}
        ObjectPoolManager.Instance.ReturnAreaListToPool(surroundingAreas);
    }
    private void PopulateFrozenCharactersInHome(List<Character> p_characterList, Character character) {
        if (character.homeSettlement?.mainStorage != null) {
            character.homeSettlement.mainStorage.PopulateCharacterListThatMeetCriteria(p_characterList, x => x.traitContainer.HasTrait("Frozen") && x.race != RACE.KOBOLD);
            //return character.homeSettlement.mainStorage.charactersHere.Where(x => x.traitContainer.HasTrait("Frozen") && x.race != RACE.KOBOLD).ToList();
        } else if (character.homeStructure != null) {
            character.homeStructure.PopulateCharacterListThatMeetCriteria(p_characterList, x => x.traitContainer.HasTrait("Frozen") && x.race != RACE.KOBOLD);
        } else if (character.HasTerritory()) {
            character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatMeetCriteria(p_characterList, c => c.traitContainer.HasTrait("Frozen") && c.race != RACE.KOBOLD);
        }
    }

    private void PopulateAreasSurroundingHome(List<Area> areas, Character character) {
        Area homeArea = null;
        if (character.homeSettlement != null) {
            character.homeSettlement.PopulateSurroundingAreas(areas);
        } else if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                homeArea = CollectionUtilities.GetRandomElement(cave.occupiedAreas);
                //return homeArea.AllNeighbours.Where(x => x.region == homeArea.region).ToList();
            } else {
                homeArea = character.homeStructure.occupiedArea;
                //return homeArea.AllNeighbours.Where(x => x.region == homeArea.region).ToList();    
            }
        } else if (character.HasTerritory()) {
            homeArea = character.territory;
            //List<HexTile> surroundingAreas = character.territory.AllNeighbours.Where(x => x.region == character.territory.region).ToList();
            //return surroundingAreas;
        }
        if (homeArea != null) {
            for (int i = 0; i < homeArea.neighbourComponent.neighbours.Count; i++) {
                Area neighbour = homeArea.neighbourComponent.neighbours[i];
                if (neighbour.region == homeArea.region) {
                    areas.Add(neighbour);
                }
            }
        }
        //return null;
    }

    private List<FoodPile> GetFoodPilesAtHome(Character character) {
        if (character.homeSettlement != null) {
            return character.homeSettlement.GetTileObjectsOfTypeThatMeetCriteria<FoodPile>(null);
        } else if (character.homeStructure != null) {
            return character.homeStructure.GetTileObjectsOfType<FoodPile>();
        } else if (character.HasTerritory()) {
            List<FoodPile> foodPiles = character.territory.tileObjectComponent.GetTileObjectsInHexTile<FoodPile>();
            return foodPiles;
        }
        return null;
    }
}