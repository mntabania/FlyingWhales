using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Region_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Generator.Map_Generation.Components {
    public class SpecialStructureGeneration : MapGenerationComponent {

	    private readonly STRUCTURE_TYPE[] specialStructuresToGenerate = new[] {
		    STRUCTURE_TYPE.MONSTER_LAIR,
		    STRUCTURE_TYPE.ABANDONED_MINE,
		    STRUCTURE_TYPE.TEMPLE,
		    STRUCTURE_TYPE.MAGE_TOWER,
		    STRUCTURE_TYPE.ANCIENT_GRAVEYARD,
		    STRUCTURE_TYPE.RUINED_ZOO,
		    STRUCTURE_TYPE.ANCIENT_RUIN,
	    };
	    
        public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
	        for (int i = 0; i < specialStructuresToGenerate.Length; i++) {
		        STRUCTURE_TYPE structureType = specialStructuresToGenerate[i];
		        int loopCount = GetLoopCount(structureType, data);
		        int chancePerLoop = GetChance(structureType);
		        List<Area> locationChoices = GetLocationChoices(structureType);
		        if (locationChoices != null && locationChoices.Count > 0) {
			        yield return MapGenerator.Instance.StartCoroutine(TryCreateSpecialStructure(structureType, locationChoices, loopCount, chancePerLoop));    
		        } else {
			        Debug.LogWarning($"Could not find areas to spawn {structureType.ToString()}");
		        }
	        }
	        SpecialStructureSecondPass();
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
		}
		#endregion

        private IEnumerator TryCreateSpecialStructure(STRUCTURE_TYPE p_structureType, List<Area> p_choices, int p_loopCount, int p_chancePerLoop) {
	        int createdCount = 0;
	        for (int i = 0; i < p_loopCount; i++) {
		        if (GameUtilities.RollChance(p_chancePerLoop)) {
			        if (p_choices.Count > 0) {
				        Area chosenArea = CollectionUtilities.GetRandomElement(p_choices);
				        STRUCTURE_TYPE structureType = GetStructureTypeToCreate(p_structureType, chosenArea);
				        chosenArea.featureComponent.RemoveAllFeatures(chosenArea);
				        chosenArea.SetElevation(ELEVATION.PLAIN);
				        p_choices.Remove(chosenArea);
				        p_choices.ListRemoveRange(chosenArea.neighbourComponent.neighbours);
				        NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(chosenArea.region, LOCATION_TYPE.DUNGEON, chosenArea);
				        yield return MapGenerator.Instance.StartCoroutine(CreateSpecialStructure(structureType, chosenArea.region, chosenArea, settlement));
				        createdCount++;
			        } else {
				        break;
			        }
		        }
	        }
	        yield return null;
	        Debug.Log($"Created {createdCount.ToString()} {p_structureType.ToString()}");
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
			LocationStructure wilderness = hexTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
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