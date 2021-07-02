using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using Locations.Region_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Generator.Map_Generation.Components {
    public class SpecialStructureGeneration : MapGenerationComponent {

	    private readonly STRUCTURE_TYPE[] specialStructureChoices = new[] {
		    STRUCTURE_TYPE.MONSTER_LAIR,
		    STRUCTURE_TYPE.ABANDONED_MINE,
		    STRUCTURE_TYPE.TEMPLE,
		    STRUCTURE_TYPE.MAGE_TOWER,
		    STRUCTURE_TYPE.ANCIENT_GRAVEYARD,
		    // STRUCTURE_TYPE.RUINED_ZOO,
		    STRUCTURE_TYPE.ANCIENT_RUIN,
	    };
	    
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
	        //Reference: https://www.notion.so/ruinarch/World-Generation-08714a9e2e574174a9336d75f10d6547#1cb5f4c8c9dc4939b6443b88fb51e1e1
	        int specialStructuresToCreate = WorldSettings.Instance.worldSettingsData.mapSettings.GetSpecialStructuresToCreate();
	        List<Area> locationChoices = RuinarchListPool<Area>.Claim();
	        for (int i = 0; i < data.unreservedAreas.Count; i++) {
		        Area area = data.unreservedAreas[i];
		        if (area.elevationComponent.IsFully(ELEVATION.PLAIN) && area.featureComponent.features.Count == 0 && area.primaryStructureInArea is Wilderness) {
			        locationChoices.Add(area);
		        }
	        }
	        List<Area> occupiedLocations = RuinarchListPool<Area>.Claim();
	        Debug.Log($"Will create {specialStructuresToCreate.ToString()} special structures. \nLocation Choices({locationChoices.Count.ToString()}): {locationChoices.ComafyList()}");
	        for (int i = 0; i < specialStructuresToCreate; i++) {
		        if (locationChoices.Count == 0) { break; }
		        STRUCTURE_TYPE structureToCreate = CollectionUtilities.GetRandomElement(specialStructureChoices);
		        Area targetArea = CollectionUtilities.GetRandomElement(locationChoices);
		        locationChoices.Remove(targetArea);
		        occupiedLocations.Add(targetArea);
		        yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(structureToCreate, targetArea));
		        Debug.Log($"Created {structureToCreate.ToString()} at {targetArea}");
	        }
	        
	        //go through each biome and add unique features
	        string summary = "Will go through biomes and add features...";
	        for (int i = 0; i < GridMap.Instance.mainRegion.biomeDivisionComponent.divisions.Count; i++) {
		        BiomeDivision biomeDivision = GridMap.Instance.mainRegion.biomeDivisionComponent.divisions[i];
		        List<Area> unreservedFullyFlatAreasInBiome = RuinarchListPool<Area>.Claim();
		        biomeDivision.PopulateUnreservedFullyFlatTiles(unreservedFullyFlatAreasInBiome, data.reservedAreas);
		        unreservedFullyFlatAreasInBiome.ListRemoveRange(occupiedLocations);
		        summary = $"{summary}\nBiome Division: {biomeDivision.biome.ToString()}";
		        if (biomeDivision.areas.Count > 0 && unreservedFullyFlatAreasInBiome.Count > 0) {
			        if ((biomeDivision.biome == BIOMES.DESERT || biomeDivision.biome == BIOMES.GRASSLAND || biomeDivision.biome == BIOMES.SNOW) && GameUtilities.RollChance(6)) {
				        //if Biome is Desert or Jungle or Snow, 6% chance that it has plenty of Ruins:
				        summary = $"{summary}\nBiome is Desert or Jungle or Snow, 6% chance that it has plenty of Ruins";
				        int count = 0;
				        for (int j = 0; j < 4; j++) {
					        if (unreservedFullyFlatAreasInBiome.Count == 0) { break; }
					        Area randomArea = CollectionUtilities.GetRandomElement(unreservedFullyFlatAreasInBiome);
					        unreservedFullyFlatAreasInBiome.Remove(randomArea);
					        Assert.IsTrue(randomArea.elevationComponent.IsFully(ELEVATION.PLAIN));
					        yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(STRUCTURE_TYPE.ANCIENT_RUIN, randomArea));
					        count++;
				        }
				        summary = $"{summary}\nCreated {count} Ruins";
			        } else if ((biomeDivision.biome == BIOMES.DESERT || biomeDivision.biome == BIOMES.SNOW) && GameUtilities.RollChance(6)) {
				        // if Biome is Desert or Snow, 6% chance that it has plenty of Ancient Graveyards:
				        summary = $"{summary}\nBiome is Desert or Snow, 6% chance that it has plenty of Ancient Graveyards";
				        int count = 0;
				        for (int j = 0; j < 4; j++) {
					        if (unreservedFullyFlatAreasInBiome.Count == 0) { break; }
					        Area randomArea = CollectionUtilities.GetRandomElement(unreservedFullyFlatAreasInBiome);
					        unreservedFullyFlatAreasInBiome.Remove(randomArea);
					        yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(STRUCTURE_TYPE.ANCIENT_GRAVEYARD, randomArea));
					        count++;
				        }
				        summary = $"{summary}\nCreated {count} Ancient Graveyards";
			        } else if (biomeDivision.biome == BIOMES.GRASSLAND && GameUtilities.RollChance(6)) {
				        //if Biome is Grassland, 6% chance to add Teeming tag to the Biome
				        summary = $"{summary}\nBiome is Grassland, 6% chance to add Teeming tag to the Biome";
				        ApplyTeemingEffect(biomeDivision, ref summary);
			        }else if (GameUtilities.RollChance(3)) {
				        //3% chance it has plenty of Poison Vents:
				        summary = $"{summary}\n3% chance it has plenty of Poison Vents";
				        Area randomArea = CollectionUtilities.GetRandomElement(unreservedFullyFlatAreasInBiome);
				        unreservedFullyFlatAreasInBiome.Remove(randomArea);
				        List<LocationGridTile> validTiles = RuinarchListPool<LocationGridTile>.Claim();
				        PopulateValidTilesForVents(validTiles, randomArea);
				        int count = 0;
				        for (int j = 0; j < 8; j++) {
					        if (validTiles.Count == 0) { break; }
					        LocationGridTile poisonVentLocation = CollectionUtilities.GetRandomElement(validTiles);
					        TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.POISON_VENT);
					        poisonVentLocation.structure.AddPOI(tileObject, poisonVentLocation);
					        validTiles.Remove(poisonVentLocation);
					        count++;
				        }
				        RuinarchListPool<LocationGridTile>.Release(validTiles);
				        summary = $"{summary}\nCreated {count} Poison Vents";
			        }else if (GameUtilities.RollChance(3)) {
				        //3% chance it has plenty of Vapor Vents:
				        summary = $"{summary}\n3% chance it has plenty of Vapor Vents";
				        Area randomArea = CollectionUtilities.GetRandomElement(unreservedFullyFlatAreasInBiome);
				        unreservedFullyFlatAreasInBiome.Remove(randomArea);
				        List<LocationGridTile> validTiles = RuinarchListPool<LocationGridTile>.Claim();
				        PopulateValidTilesForVents(validTiles, randomArea);
				        int count = 0;
				        for (int j = 0; j < 8; j++) {
					        if (validTiles.Count == 0) { break; }
					        LocationGridTile vaporVentLocation = CollectionUtilities.GetRandomElement(validTiles);
					        TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.VAPOR_VENT);
					        vaporVentLocation.structure.AddPOI(tileObject, vaporVentLocation);
					        validTiles.Remove(vaporVentLocation);
					        count++;
				        }
				        RuinarchListPool<LocationGridTile>.Release(validTiles);
				        summary = $"{summary}\nCreated {count} Vapor Vents";
			        }
		        }
		        RuinarchListPool<Area>.Release(unreservedFullyFlatAreasInBiome);
	        }
	        RuinarchListPool<Area>.Release(occupiedLocations);
	        RuinarchListPool<Area>.Release(locationChoices);
	        Debug.Log(summary);
	        // for (int i = 0; i < specialStructureChoices.Length; i++) {
	        //  STRUCTURE_TYPE structureType = specialStructureChoices[i];
	        //  int loopCount = GetLoopCount(structureType, data);
	        //  int chancePerLoop = GetChance(structureType);
	        //  List<Area> locationChoices = GetLocationChoices(structureType);
	        //  if (locationChoices != null && locationChoices.Count > 0) {
	        //   yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(structureType, locationChoices, loopCount, chancePerLoop));    
	        //  } else {
	        //   Debug.LogWarning($"Could not find areas to spawn {structureType.ToString()}");
	        //  }
	        // }
	        SpecialStructureSecondPass();

	        AdditionalResourceCreation();
        }
        private void PopulateValidTilesForVents(List<LocationGridTile> p_tiles, Area p_area) {
	        for (int i = 0; i < p_area.gridTileComponent.passableTiles.Count; i++) {
		        LocationGridTile tile = p_area.gridTileComponent.passableTiles[i];
		        if (tile.tileObjectComponent.objHere == null && !tile.isOccupied && tile.mainBiomeType == p_area.biomeType) {
			        p_tiles.Add(tile);
		        }
	        }
        }
        private void ApplyTeemingEffect(BiomeDivision p_biomeDivision, ref string p_summary) {
	        List<GameFeature> gameFeatures = RuinarchListPool<GameFeature>.Claim();
	        PopulateGameFeaturesInBiomeDivision(gameFeatures, p_biomeDivision);
	        if (gameFeatures.Count < 6) {
		        int missing = UnityEngine.Random.Range(6, 10) - gameFeatures.Count;
		        //choose from random flat/tree tile without game feature
		        List<Area> choices = RuinarchListPool<Area>.Claim();
		        for (int i = 0; i < p_biomeDivision.areas.Count; i++) {
			        Area currArea = p_biomeDivision.areas[i];
			        if(currArea.elevationType == ELEVATION.PLAIN && !currArea.featureComponent.HasFeature(AreaFeatureDB.Game_Feature)) {
				        choices.Add(currArea);
			        }
		        }
		        p_summary = $"{p_summary}\nWill add game feature to {missing.ToString()} areas:";
		        int count = 0;
		        for (int i = 0; i < missing; i++) {
			        if (choices.Count == 0) { break; }
			        Area chosenArea = CollectionUtilities.GetRandomElement(choices);
			        GameFeature feature = LandmarkManager.Instance.CreateAreaFeature<GameFeature>(AreaFeatureDB.Game_Feature);
			        chosenArea.featureComponent.AddFeature(feature, chosenArea);
			        gameFeatures.Add(feature);
			        choices.Remove(chosenArea);
			        p_summary = $"{p_summary}|Added game feature to {chosenArea}|"; 
			        count++;
		        }
		        p_summary = $"{p_summary}\nAdded game feature to {count.ToString()} areas"; 
		        RuinarchListPool<Area>.Release(choices);
	        }

	        //set spawn type to same for every feature
	        SUMMON_TYPE animalType = CollectionUtilities.GetRandomElement(GameFeature.spawnChoices);
	        for (int i = 0; i < gameFeatures.Count; i++) {
		        GameFeature gameFeature = gameFeatures[i];
		        gameFeature.SetSpawnType(animalType);
	        }
        }
        private void PopulateGameFeaturesInBiomeDivision(List<GameFeature> p_gameFeatures, BiomeDivision p_biomeDivision) {
	        for (int i = 0; i < p_biomeDivision.areas.Count; i++) {
		        Area area = p_biomeDivision.areas[i];
		        GameFeature feature = area.featureComponent.GetFeature<GameFeature>();
		        if (feature != null) {
			        p_gameFeatures.Add(feature);
		        }
	        }
        }
        private void SpecialStructureSecondPass() {
			for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
				Region region = GridMap.Instance.allRegions[i];
				for (int j = 0; j < region.regionFeatureComponent.features.Count; j++) {
					RegionFeature feature = region.regionFeatureComponent.features[j];
					feature.SpecialStructureGenerationSecondPassActions(region);
				}
			}
		}
        
        #region Scenario Maps
		public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
			// List<SaveDataArea> landmarks = scenarioMapData.worldMapSave.GetAllTilesWithLandmarks();
			// for (int i = 0; i < landmarks.Count; i++) {
			// 	SaveDataArea landmark = landmarks[i];
			// 	Area area = GridMap.Instance.map[landmark.xCoordinate, landmark.yCoordinate];
			// 	if (landmark.landmarkType.GetStructureType().IsSpecialStructure()) {
			// 		NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
			// 		Assert.IsTrue(settlement.areas.Count > 0, $"{settlement.name} has no areas to place {landmark.landmarkType.ToString()}");
			// 		yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(landmark.landmarkType.GetStructureType(), area.region, area, settlement));	
			// 	}
			// }
			
			for (int i = 0; i < scenarioMapData.worldMapSave.specialStructureSaves.Count; i++) {
				SpecialStructureSetting specialStructureSetting = scenarioMapData.worldMapSave.specialStructureSaves[i];
				Area area = GridMap.Instance.map[specialStructureSetting.location.X, specialStructureSetting.location.Y];
				NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(area.region, LOCATION_TYPE.DUNGEON, area);
				yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(specialStructureSetting.structureType, area.region, area,settlement));
			}
			yield return null;
			if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
				AdditionalResourceCreationForOona();
			} else {
				AdditionalResourceCreation();	
			}
			
		}
		#endregion

        // private IEnumerator TryCreateSpecialStructure(STRUCTURE_TYPE p_structureType, List<Area> p_choices, int p_loopCount, int p_chancePerLoop) {
	       //  int createdCount = 0;
	       //  for (int i = 0; i < p_loopCount; i++) {
		      //   if (GameUtilities.RollChance(p_chancePerLoop)) {
			     //    if (p_choices.Count > 0) {
				    //     Area chosenArea = CollectionUtilities.GetRandomElement(p_choices);
				    //     STRUCTURE_TYPE structureType = GetStructureTypeToCreate(p_structureType, chosenArea);
				    //     // chosenArea.featureComponent.RemoveAllFeatures(chosenArea);
				    //     chosenArea.SetElevation(ELEVATION.PLAIN);
				    //     p_choices.Remove(chosenArea);
				    //     p_choices.ListRemoveRange(chosenArea.neighbourComponent.neighbours);
				    //     NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(chosenArea.region, LOCATION_TYPE.DUNGEON, chosenArea);
				    //     yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(structureType, chosenArea.region, chosenArea, settlement));
				    //     createdCount++;
			     //    } else {
				    //     break;
			     //    }
		      //   }
	       //  }
	       //  yield return null;
#if DEBUG_LOG
	       //  Debug.Log($"Created {createdCount.ToString()} {p_structureType.ToString()}"); 
        // }
#endif
        private IEnumerator TryCreateSpecialStructure(STRUCTURE_TYPE p_structureType, Area p_area) {
	        // chosenArea.featureComponent.RemoveAllFeatures(chosenArea);
	        NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(p_area.region, LOCATION_TYPE.DUNGEON, p_area);
	        yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(p_structureType, p_area.region, p_area, settlement));
        }

		#region Structure Creation
		private IEnumerator CreateSpecialStructure(STRUCTURE_TYPE p_structureType, Region p_region, Area p_area, NPCSettlement p_settlement) {
			if (p_structureType == STRUCTURE_TYPE.MONSTER_LAIR) {
				LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(p_region, p_structureType, p_settlement);
				yield return MapGenerator.Instance.StartCoroutine(GenerateMonsterLair(p_area, structure));
			} else {
				yield return MapGenerator.Instance.StartCoroutine(LandmarkManager.Instance.PlaceBuiltLandmark(p_settlement, p_region.innerMap, RESOURCE.NONE, p_structureType));
			}
		}
		private IEnumerator GenerateMonsterLair(Area hexTile, LocationStructure structure) {
			List<LocationGridTile> locationGridTiles = new List<LocationGridTile>(hexTile.gridTileComponent.gridTiles);
			LocationStructure wilderness = hexTile.region.wilderness;
			InnerMapManager.Instance.MonsterLairCellAutomata(locationGridTiles, structure, hexTile.region, wilderness);
			structure.SetOccupiedArea(hexTile);
			yield return null;
		}
		#endregion
		
		#region Chances
		private int GetLoopCount(STRUCTURE_TYPE p_structureType, MapGenerationData data) {
			switch (p_structureType) {
				case STRUCTURE_TYPE.MONSTER_LAIR:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 2;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 0;
					} else {
						if (data.regionCount == 1) {
							return 1;
						} else if (data.regionCount == 2 || data.regionCount == 3) {
							return 2;
						} else {
							return 3;
						}	
					}
				case STRUCTURE_TYPE.ABANDONED_MINE:
					if (WorldConfigManager.Instance.isTutorialWorld) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 0;
					}
					bool monsterLairWasBuilt = GridMap.Instance.mainRegion.HasStructure(STRUCTURE_TYPE.MONSTER_LAIR);
					if (data.regionCount == 1) {
						return monsterLairWasBuilt ? 0 : 1;
					} else if (data.regionCount == 2 || data.regionCount == 3) {
						return monsterLairWasBuilt ? 1 : 2;
					} else {
						return monsterLairWasBuilt ? 2 : 3;
					}
				case STRUCTURE_TYPE.TEMPLE:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 1;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 5;
					} else {
						if (data.regionCount == 1) {
							return 1;
						} else if (data.regionCount == 2 || data.regionCount == 3) {
							return 2;
						} else {
							return 3;
						}
					}
				case STRUCTURE_TYPE.MAGE_TOWER:
					if (WorldConfigManager.Instance.isTutorialWorld) {
						return 0;
					}  if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 0;
					} 
					bool templeWasBuilt = GridMap.Instance.mainRegion.HasStructure(STRUCTURE_TYPE.ANCIENT_RUIN);
					if (data.regionCount == 1) {
						return templeWasBuilt ? 0 : 1;
					} else if (data.regionCount == 2 || data.regionCount == 3) {
						return templeWasBuilt ? 1 : 2;
					} else {
						return 3;
					}
				case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 2;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						//always ensure that an ancient graveyard is spawned in second world
						return 1;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
						return 3;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 0;
					} else {
						return 0;
					}
				case STRUCTURE_TYPE.RUINED_ZOO:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 1;
					} else {
						return 0;
					}
				case STRUCTURE_TYPE.ANCIENT_RUIN:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 1;
					} else {
						return 0;
					}
				default:
					return 0;
			}
		}
		private int GetChance(STRUCTURE_TYPE p_structureType) {
			switch (p_structureType) {
				case STRUCTURE_TYPE.MONSTER_LAIR:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 100;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 0;
					} else {
						return 75;
					}
				case STRUCTURE_TYPE.ABANDONED_MINE:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 100;
					} else {
						return 50;
					}
				case STRUCTURE_TYPE.TEMPLE:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 100;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 100;
					} else {
						return 35;
					}
				case STRUCTURE_TYPE.MAGE_TOWER:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 100;
					} else {
						return 35;
					}
				case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return 100;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return 0;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
						return 75;
					} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
						return 100;
					} else {
						return 0;
					}
				case STRUCTURE_TYPE.RUINED_ZOO:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 100;
					} else {
						return 0;
					}
				case STRUCTURE_TYPE.ANCIENT_RUIN:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return 100;
					} else {
						return 0;
					}
				default:
					return 0;
			}
		}
		#endregion
		
		#region Utilities
		private void AdditionalResourceCreation() {
			//Reference: https://trello.com/c/rR5Bmg0h/5020-set-special-resources-per-village-to-just-1
			int randomResourceCount = GridMap.Instance.mainRegion.villageSpots.Count;
			// for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
			// 	randomResourceCount += UnityEngine.Random.Range(1, 4);
			// }
			List<Area> validAreasNotInVillageSpot = RuinarchListPool<Area>.Claim();
			List<Area> areasInVillageSpot = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
				Area area = GridMap.Instance.allAreas[i];
				if (area.GetOccupyingVillageSpot() == null) {
					//make sure that area is not next to a spot that is reserved by a village
					List<Area> areasInRange = RuinarchListPool<Area>.Claim();
					area.PopulateAreasInRange(areasInRange, 1);
					if (areasInRange.All(a => a.GetOccupyingVillageSpot() == null)) {
						validAreasNotInVillageSpot.Add(area);	
					}
					RuinarchListPool<Area>.Release(areasInRange);
				} else {
					areasInVillageSpot.Add(area);
				}
			}

			List<string> randomResourceChoices = RuinarchListPool<string>.Claim();
			randomResourceChoices.Add("BOAR_DEN");
			randomResourceChoices.Add("WOLF_DEN");
			randomResourceChoices.Add("BEAR_DEN");
	        randomResourceChoices.Add("RABBIT_HOLE");
	        randomResourceChoices.Add("Game Feature");
	        randomResourceChoices.Add("MINK_HOLE");
	        randomResourceChoices.Add("MOONCRAWLER_HOLE");

	        
	        for (int i = 0; i < randomResourceCount; i++) {
		        string randomType = CollectionUtilities.GetRandomElement(randomResourceChoices);

		        if (randomType == "Game Feature" || randomType == "RABBIT_HOLE" || randomType == "MINK_HOLE" || randomType == "MOONCRAWLER_HOLE") {
			        if (areasInVillageSpot.Count > 0) {
				        Area randomArea = CollectionUtilities.GetRandomElement(areasInVillageSpot);
				        areasInVillageSpot.Remove(randomArea);
				        if (randomType == "Game Feature") {
					        randomArea.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, randomArea);    
				        } else {
					        CreateMonsterDen(randomType, randomArea);
				        }
			        }
		        } else {
			        if (validAreasNotInVillageSpot.Count > 0) {
				        Area randomArea = CollectionUtilities.GetRandomElement(validAreasNotInVillageSpot);
						validAreasNotInVillageSpot.Remove(randomArea);
						CreateMonsterDen(randomType, randomArea);
			        }
		        }
	        }
	        RuinarchListPool<string>.Release(randomResourceChoices);
	        RuinarchListPool<Area>.Release(areasInVillageSpot);
	        RuinarchListPool<Area>.Release(validAreasNotInVillageSpot);
		}
		private void AdditionalResourceCreationForOona() {
			int randomResourceCount = 0;
			for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
				randomResourceCount += 5;
			}
			List<Area> validAreasNotInVillageSpot = RuinarchListPool<Area>.Claim();
			List<Area> areasInVillageSpot = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < GridMap.Instance.allAreas.Count; i++) {
				Area area = GridMap.Instance.allAreas[i];
				if (area.GetOccupyingVillageSpot() == null) {
					//make sure that area is not next to a spot that is reserved by a village
					List<Area> areasInRange = RuinarchListPool<Area>.Claim();
					area.PopulateAreasInRange(areasInRange, 1);
					if (areasInRange.All(a => a.GetOccupyingVillageSpot() == null)) {
						validAreasNotInVillageSpot.Add(area);	
					}
					RuinarchListPool<Area>.Release(areasInRange);
				} else {
					areasInVillageSpot.Add(area);
				}
			}

			List<string> randomResourceChoices = RuinarchListPool<string>.Claim();
			randomResourceChoices.Add("BOAR_DEN");
			randomResourceChoices.Add("WOLF_DEN");
			randomResourceChoices.Add("BEAR_DEN");
	        randomResourceChoices.Add("RABBIT_HOLE");
	        randomResourceChoices.Add("Game Feature");
	        randomResourceChoices.Add("MINK_HOLE");
	        randomResourceChoices.Add("MOONCRAWLER_HOLE");

	        
	        for (int i = 0; i < randomResourceCount; i++) {
		        string randomType = CollectionUtilities.GetRandomElement(randomResourceChoices);

		        if (randomType == "Game Feature" || randomType == "RABBIT_HOLE" || randomType == "MINK_HOLE" || randomType == "MOONCRAWLER_HOLE") {
			        if (areasInVillageSpot.Count > 0) {
				        Area randomArea = CollectionUtilities.GetRandomElement(areasInVillageSpot);
				        areasInVillageSpot.Remove(randomArea);
				        if (randomType == "Game Feature") {
					        randomArea.featureComponent.AddFeature(AreaFeatureDB.Game_Feature, randomArea);    
				        } else {
					        CreateMonsterDen(randomType, randomArea);
				        }
			        }
		        } else {
			        if (validAreasNotInVillageSpot.Count > 0) {
				        Area randomArea = CollectionUtilities.GetRandomElement(validAreasNotInVillageSpot);
						validAreasNotInVillageSpot.Remove(randomArea);
						CreateMonsterDen(randomType, randomArea);
			        }
		        }
	        }
	        RuinarchListPool<string>.Release(randomResourceChoices);
	        RuinarchListPool<Area>.Release(areasInVillageSpot);
	        RuinarchListPool<Area>.Release(validAreasNotInVillageSpot);
		}

		private void CreateMonsterDen(string randomType, Area randomArea) {
			STRUCTURE_TYPE structureType = (STRUCTURE_TYPE) Enum.Parse(typeof(STRUCTURE_TYPE), randomType);
			GameObject structurePrefab = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(new StructureSetting(structureType, RESOURCE.NONE));

			List<LocationGridTile> unoccupiedTiles = RuinarchListPool<LocationGridTile>.Claim();
			for (int j = 0; j < randomArea.gridTileComponent.gridTiles.Count; j++) {
				LocationGridTile tile = randomArea.gridTileComponent.gridTiles[j];
				if (tile.structure is Wilderness && tile.tileObjectComponent.objHere == null && tile.IsPassable()) {
					List<LocationGridTile> overlappedTiles = RuinarchListPool<LocationGridTile>.Claim();
					tile.PopulateTilesInRadius(overlappedTiles, 2, includeCenterTile: true,
						includeTilesInDifferentStructure: true);
					if (!overlappedTiles.Any(t => t.structure.structureType != STRUCTURE_TYPE.WILDERNESS || t.IsAtEdgeOfMap() || !t.IsPassable())) {
						unoccupiedTiles.Add(tile);
					}

					RuinarchListPool<LocationGridTile>.Release(overlappedTiles);
				}
			}

			if (unoccupiedTiles.Count > 0) {
				LocationGridTile randomLocation = CollectionUtilities.GetRandomElement(unoccupiedTiles);
				NPCSettlement settlement =
					LandmarkManager.Instance.CreateNewSettlement(randomArea.region, LOCATION_TYPE.DUNGEON, randomArea);
				LocationStructure structure = LandmarkManager.Instance.PlaceIndividualBuiltStructureForSettlement(settlement,
					GridMap.Instance.mainRegion.innerMap, structurePrefab, randomLocation);
				Debug.Log($"Added animal den - {randomType.ToString()} to {randomLocation.ToString()}");
			}

			RuinarchListPool<LocationGridTile>.Release(unoccupiedTiles);
		}

		private List<Area> GetLocationChoices(STRUCTURE_TYPE p_structureType) {
			switch (p_structureType) {
				case STRUCTURE_TYPE.MONSTER_LAIR:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return new List<Area>() { GridMap.Instance.map[1, 4], GridMap.Instance.map[3, 7] };
					} else {
						return GridMap.Instance.allAreas.Where(x => 
								x.elevationComponent.IsFully(ELEVATION.PLAIN) && //a random flat tile
								x.featureComponent.features.Count == 0 && x.primaryStructureInArea is Wilderness && //with no Features yet
								!IsInRangeOfSettlement(x, 3) && !IsAdjacentToNonCaveSpecialStructure(x) //and not adjacent to Settlement or other non-cave landmarks
						).ToList();
					}
				case STRUCTURE_TYPE.ABANDONED_MINE:
					return GridMap.Instance.allAreas.Where(x => 
							x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.featureComponent.features.Count == 0 && 
					        x.neighbourComponent.HasNeighbourWithElevation(ELEVATION.MOUNTAIN) && x.primaryStructureInArea is Wilderness &&  
					        !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
						).ToList();
				case STRUCTURE_TYPE.TEMPLE:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
						return new List<Area>() { GridMap.Instance.map[6, 1] };	
					} else {
						return GridMap.Instance.allAreas.Where(x => 
								x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.featureComponent.features.Count == 0 && x.primaryStructureInArea is Wilderness && 
						        !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
							).ToList();
					}
				case STRUCTURE_TYPE.MAGE_TOWER:
					return GridMap.Instance.allAreas.Where(x => 
							x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.featureComponent.features.Count == 0 && x.primaryStructureInArea is Wilderness &&  
					        !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
						).ToList();
				case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
						return new List<Area>() {GridMap.Instance.map[2, 2]};
					} else {
						return GridMap.Instance.allAreas.Where(x => 
								x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.primaryStructureInArea is Wilderness &&
							    !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
							).ToList();
					}
				case STRUCTURE_TYPE.RUINED_ZOO:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return new List<Area>() {GridMap.Instance.map[2, 3]};
					} else {
						return GridMap.Instance.allAreas.Where(x => 
								x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.primaryStructureInArea is Wilderness &&
						        !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
							).ToList();
					}
				case STRUCTURE_TYPE.ANCIENT_RUIN:
					if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Icalawa) {
						return new List<Area>() {GridMap.Instance.map[3, 0]};
					} else {
						return GridMap.Instance.allAreas.Where(x => 
								x.elevationComponent.IsFully(ELEVATION.PLAIN) && x.primaryStructureInArea is Wilderness &&
						        !IsAdjacentToVillage(x) && !IsAdjacentToNonCaveSpecialStructure(x)//and not adjacent to Settlement or other non-cave landmarks
							).ToList();
					}
			}
			return null;
		}
		private STRUCTURE_TYPE GetStructureTypeToCreate(STRUCTURE_TYPE p_preferredStructure, Area p_area) {
			if (p_area.region.regionFeatureComponent.HasFeature<RuinsFeature>()) {
				return STRUCTURE_TYPE.ANCIENT_RUIN;
			} else if (p_area.region.regionFeatureComponent.HasFeature<HauntedFeature>()) {
				return STRUCTURE_TYPE.ANCIENT_GRAVEYARD;
			}
			return p_preferredStructure;
		}
		private bool IsAdjacentToVillage(Area p_area) {
			for (int i = 0; i < p_area.neighbourComponent.neighbours.Count; i++) {
				Area neighbour = p_area.neighbourComponent.neighbours[i];
				if (neighbour.settlementOnArea != null && neighbour.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
					return true;
				}
			}
			return false;
		}
		private bool IsInRangeOfSettlement(Area tile, int range) {
			List<Area> tilesInRange = RuinarchListPool<Area>.Claim();
			tile.PopulateAreasInRange(tilesInRange, range);
			bool isInRange = false;
			for (int i = 0; i < tilesInRange.Count; i++) {
				Area tileInRange = tilesInRange[i];
				if (tileInRange.settlementOnArea != null && tileInRange.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
					isInRange = true;
					break;
				}
			}
			RuinarchListPool<Area>.Release(tilesInRange);
			return isInRange;
		}
		private bool IsAdjacentToNonCaveSpecialStructure(Area p_area) {
			for (int i = 0; i < p_area.neighbourComponent.neighbours.Count; i++) {
				Area neighbour = p_area.neighbourComponent.neighbours[i];
				if (neighbour.settlementOnArea != null && neighbour.settlementOnArea.locationType == LOCATION_TYPE.DUNGEON && 
				    neighbour.primaryStructureInArea.structureType != STRUCTURE_TYPE.CAVE) {
					return true;
				}
			}
			return false;
		}
		#endregion
    }
}