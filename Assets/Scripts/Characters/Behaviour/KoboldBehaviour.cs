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
#if DEBUG_LOG
        log += $"\n{character.name} is Kobold";
#endif
        if (ChanceData.RollChance(CHANCE_TYPE.Kobold_Place_Freezing_Trap, ref log)) { //10
#if DEBUG_LOG
            log += $"\nChance to place freezing trap met.";
#endif
#if DEBUG_PROFILER
            Profiler.BeginSample($"Kobold Place Freezing Trap");
#endif

            List<Area> areaChoices = ObjectPoolManager.Instance.CreateNewAreaList();
            PopulateValidHexTilesNextToHome(areaChoices, character);
            if (areaChoices.Count > 0) {
                Area chosenArea = CollectionUtilities.GetRandomElement(areaChoices);
                ObjectPoolManager.Instance.ReturnAreaListToPool(areaChoices);
                LocationGridTile targetTile = chosenArea.gridTileComponent.GetRandomUnoccupiedNoFreezingTrapNotNextToSettlementTile();
                //List<LocationGridTile> locationGridTileChoices = chosenArea.gridTileComponent.gridTiles.Where(x => 
                //        x.tileObjectComponent.hasFreezingTrap == false && x.isOccupied == false && x.IsNextToSettlement() == false).ToList();
                if (targetTile != null) {
                    //LocationGridTile targetTile = CollectionUtilities.GetRandomElement(locationGridTileChoices);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, INTERACTION_TYPE.PLACE_FREEZING_TRAP, targetTile.tileObjectComponent.genericTileObject, character);
                    producedJob = job;
#if DEBUG_LOG
                    log += $"\nCreated job to place trap at {targetTile}.";
#endif
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                    return true;
                } else {
#if DEBUG_LOG
                    log += $"\nNo valid tiles at {chosenArea} to place trap.";
#endif
                    producedJob = null;
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                    return false;
                }
            } else {
                ObjectPoolManager.Instance.ReturnAreaListToPool(areaChoices);
#if DEBUG_LOG
                log += $"\nNo valid areas to place freezing traps found.";
#endif
                producedJob = null;
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
                return false;
            }
        } else {
#if DEBUG_LOG
            log += $"\nChance to place freezing trap NOT met.";
#endif
#if DEBUG_PROFILER
            Profiler.BeginSample($"Kobold Get Frozen Characters Surrounding Home");
#endif
            List<Character> frozenCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
            PopulateFrozenCharactersSurroundingHome(frozenCharacters, character);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
            if (frozenCharacters.Count > 0) {
                //check if a character is frozen in any of the neighbouring areas,
                //if there are, then create a job to carry then drop them at this character's home/territory
#if DEBUG_PROFILER
                Profiler.BeginSample($"Kobold Capture Frozen Characters");
#endif
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
#if DEBUG_LOG
                log += $"\nFrozen character at surrounding area found, will carry {chosenCharacter.name} and drop at home.";
#endif
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
                return true;
            } else {
#if DEBUG_LOG
                log += $"\nNo frozen characters at surrounding area found, checking frozen characters at home.";
#endif
//if there are none, check if there are any characters inside this character's home/territory that is frozen
#if DEBUG_PROFILER
                Profiler.BeginSample($"Kobold Populate Frozen Characters in Home");
#endif
                PopulateFrozenCharactersInHome(frozenCharacters, character);
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
                if (frozenCharacters.Count > 0) {
#if DEBUG_LOG
                    log += $"\nFrozen characters at home found.";
#endif
                    //if there are, 8% chance to butcher one, otherwise mock or laugh at one
                    Character chosenCharacter = CollectionUtilities.GetRandomElement(frozenCharacters);
                    if (GameUtilities.RollChance(8)) {
#if DEBUG_LOG
                        log += $"\nChance to butcher met, will butcher {chosenCharacter.name}.";
#endif
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_BUTCHER, INTERACTION_TYPE.BUTCHER, chosenCharacter, character);
                        job.SetCancelOnDeath(false);
                        ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
                        producedJob = job;
                        return true;
                    } else {
#if DEBUG_LOG
                        log += $"\nChance to butcher NOT met, will roll Mock/Laugh At {chosenCharacter.name} instead.";
#endif
                        if (GameUtilities.RollChance(30) && character.marker.IsPOIInVision(chosenCharacter)) {
#if DEBUG_LOG
                            log += $"\nMock/Laugh triggered.";
#endif
                            character.interruptComponent.TriggerInterrupt(GameUtilities.RollChance(50) ? INTERRUPT.Mock : INTERRUPT.Laugh_At, chosenCharacter);
                            ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
                            producedJob = null;
                            return true;    
                        }
                    }
                }
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(frozenCharacters);
#if DEBUG_LOG
            log += $"\nNo jobs related to frozen characters created. Checking food piles at home";
#endif
            //if none of the jobs above were created, check for food piles inside this character's home/territory,
            if (GameUtilities.RollChance(15)) {
#if DEBUG_PROFILER
                Profiler.BeginSample($"Kobold Get Food Piles at Home");
#endif
                List<TileObject> foodPiles = RuinarchListPool<TileObject>.Claim();
                PopulateFoodPilesAtHome(foodPiles, character);
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif
                FoodPile chosenPile = null;
                if (foodPiles.Count > 0) {
                    chosenPile = CollectionUtilities.GetRandomElement(foodPiles) as FoodPile;
                }
                RuinarchListPool<TileObject>.Release(foodPiles);
                if (chosenPile != null) {
#if DEBUG_PROFILER
                    Profiler.BeginSample($"Kobold Monster Eat Job");
#endif
                    //if there are any, create job to eat a random food pile
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT, chosenPile, character);
                    producedJob = job;
#if DEBUG_LOG
                    log += $"\nFood Piles found, will eat {chosenPile}";
#endif
#if DEBUG_PROFILER
                    Profiler.EndSample();
#endif
                    return true;
                }
            }
#if DEBUG_LOG
            log += $"\nNo food piles found, will roam around territory";
#endif
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            
        }
    }
    private void PopulateValidHexTilesNextToHome(List<Area> areas, Character character) {
        Area homeArea = null;
        if (character.homeSettlement != null) {
            character.homeSettlement.PopulateSurroundingAreasInSameRegionWithLessThanNumOfFreezingTraps(areas, character.homeRegion, 2);
        } else if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                homeArea = CollectionUtilities.GetRandomElement(cave.occupiedAreas.Keys);
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
                if (neighbour.region == homeArea.region && neighbour.freezingTraps < 2) {
                    areas.Add(neighbour);
                }
            }
        }
        //return null;
    }
    private void PopulateFrozenCharactersSurroundingHome(List<Character> p_characterList, Character character) {
        List<Area> surroundingAreas = RuinarchListPool<Area>.Claim();
        PopulateAreasSurroundingHome(surroundingAreas, character);
        //if (surroundingAreas != null) {
            for (int i = 0; i < surroundingAreas.Count; i++) {
                Area area = surroundingAreas[i];
                area.locationCharacterTracker.PopulateCharacterListInsideHexForKoboldBehaviour(p_characterList);
            }
        //}
        RuinarchListPool<Area>.Release(surroundingAreas);
    }
    private void PopulateFrozenCharactersInHome(List<Character> p_characterList, Character character) {
        if (character.homeSettlement?.mainStorage != null) {
            character.homeSettlement.mainStorage.PopulateCharacterListThatIsFrozenAndNotKobold(p_characterList);
            //return character.homeSettlement.mainStorage.charactersHere.Where(x => x.traitContainer.HasTrait("Frozen") && x.race != RACE.KOBOLD).ToList();
        } else if (character.homeStructure != null) {
            character.homeStructure.PopulateCharacterListThatIsFrozenAndNotKobold(p_characterList);
        } else if (character.HasTerritory()) {
            character.territory.locationCharacterTracker.PopulateCharacterListInsideHexThatHasTraitAndNotRace(p_characterList, "Frozen", RACE.KOBOLD);
        }
    }

    private void PopulateAreasSurroundingHome(List<Area> areas, Character character) {
        Area homeArea = null;
        if (character.homeSettlement != null) {
            character.homeSettlement.PopulateSurroundingAreas(areas);
        } else if (character.homeStructure != null) {
            if (character.homeStructure is Cave cave) {
                homeArea = CollectionUtilities.GetRandomElement(cave.occupiedAreas.Keys);
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

    private void PopulateFoodPilesAtHome(List<TileObject> foodPiles, Character character) {
        if (character.homeSettlement != null) {
            character.homeSettlement.PopulateTileObjectsOfType<FoodPile>(foodPiles);
        } else if (character.homeStructure != null) {
            character.homeStructure.PopulateTileObjectsOfType<FoodPile>(foodPiles);
        } else if (character.HasTerritory()) {
            character.territory.tileObjectComponent.PopulateTileObjectsInArea<FoodPile>(foodPiles);
        }
    }
}