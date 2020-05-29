using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Placing monsters...");
		yield return MapGenerator.Instance.StartCoroutine(RegionalMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(LandmarkMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(CaveMonsterGeneration());
		yield return null;
	}

	#region Helpers
	private void CreateMonster(SUMMON_TYPE summonType, BaseSettlement settlementOnTile, BaseLandmark monsterLair,
		LocationStructure monsterLairStructure) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, settlementOnTile, monsterLair.tileLocation.region, monsterLairStructure);
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles);
		CharacterManager.Instance.PlaceSummon(summon, targetTile);
		//summon.AddTerritory(monsterLair.tileLocation);
        // summon.MigrateHomeStructureTo(monsterLairStructure);
        //summon.ChangeHomeStructure(monsterLairStructure);

        //if (monsterLairStructure is LocationStructure homeStructure) {
        //	summon.MigrateHomeStructureTo(homeStructure);	
        //}
    }
    private Summon CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, params HexTile[] territories) {
		var chosenTile = homeStructure != null ? CollectionUtilities.GetRandomElement(homeStructure.unoccupiedTiles) : CollectionUtilities.GetRandomElement(locationChoices);
		Assert.IsNotNull(chosenTile, $"Chosen tile for {summonType.ToString()} is null!");
		Assert.IsTrue(chosenTile.collectionOwner.isPartOfParentRegionMap, $"Chosen tile for {summonType.ToString()} is not part of the region map!");
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, null, chosenTile.parentMap.region);
		CharacterManager.Instance.PlaceSummon(summon, chosenTile);
		if (homeStructure != null) {
			summon.MigrateHomeStructureTo(homeStructure);	
		} else {
			summon.AddTerritory(chosenTile.collectionOwner.partOfHextile.hexTileOwner);
			if (territories != null) {
				for (int i = 0; i < territories.Length; i++) {
					HexTile territory = territories[i];
					summon.AddTerritory(territory);
				}
			}	
		}


		//summon.ChangeHomeStructure(homeStructure);
  //      if (homeStructure is IDwelling structure) {
		//	summon.MigrateHomeStructureTo(structure);
		//}
		return summon;
	}
	#endregion
	

	private IEnumerator RegionalMonsterGeneration() {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			List<LocationGridTile> locationChoices = new List<LocationGridTile>();
			region.tiles.Where(h => h.landmarkOnTile == null && 
                (h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES) && 
                h.HasOwnedSettlementNeighbour() == false).ToList().
				ForEach(h => locationChoices.AddRange(h.locationGridTiles));
			if (locationChoices.Count == 0) {
				Debug.LogWarning($"Could not find valid tiles to place monsters at {region.name}");
				continue;
			}

			if (WorldConfigManager.Instance.isDemoWorld) {
				//nymphs
				int randomNymphs = 2;
				SUMMON_TYPE[] nymphChoices = new[]
					{SUMMON_TYPE.Ice_Nymph, SUMMON_TYPE.Water_Nymph, SUMMON_TYPE.Wind_Nymph};
				for (int k = 0; k < randomNymphs; k++) {
					if (locationChoices.Count == 0) { break; }
					Summon summon = CreateMonster(CollectionUtilities.GetRandomElement(nymphChoices), locationChoices);
					locationChoices.Remove(summon.gridTileLocation);
				}
				//sludge
				int randomSludge = 3;
				for (int k = 0; k < randomSludge; k++) {
					if (locationChoices.Count == 0) { break; }
					Summon summon = CreateMonster(SUMMON_TYPE.Sludge, locationChoices);
					locationChoices.Remove(summon.gridTileLocation);
				}
				//wisps
				int randomWisp = 3;
				SUMMON_TYPE[] wispChoices = new[]
					{SUMMON_TYPE.Earthen_Wisp, SUMMON_TYPE.Electric_Wisp, SUMMON_TYPE.Fire_Wisp};
				for (int k = 0; k < randomWisp; k++) {
					if (locationChoices.Count == 0) { break; }
					Summon summon = CreateMonster(CollectionUtilities.GetRandomElement(wispChoices), locationChoices);
					locationChoices.Remove(summon.gridTileLocation);
				}
			}
			else {
				MonsterGenerationSetting monsterGenerationSetting =
					WorldConfigManager.Instance.worldWideMonsterGenerationSetting;
				List<MonsterSetting> monsterChoices = monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
				if (monsterChoices != null) {
					int iterations = monsterGenerationSetting.iterations.Random();
					for (int j = 0; j < iterations; j++) {
						MonsterSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(monsterChoices);
						int randomAmount = randomMonsterSetting.minMaxRange.Random();
						for (int k = 0; k < randomAmount; k++) {
							Summon summon = CreateMonster(randomMonsterSetting.monsterType, locationChoices);
							locationChoices.Remove(summon.gridTileLocation);
						}
						if (locationChoices.Count == 0) {
							Debug.LogWarning($"Ran out of grid tiles to place monsters at region {region.name}");
							break;
						}
					}	
				}	
			}
			
			yield return null;
		}
	}
	private IEnumerator LandmarkMonsterGeneration() {
		if (WorldConfigManager.Instance.isDemoWorld) {
			//wolves at monster lair
			List<BaseLandmark> monsterLairs = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.MONSTER_LAIR);
			for (int i = 0; i < monsterLairs.Count; i++) {
				BaseLandmark landmark = monsterLairs[i];
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				int randomAmount = 4;
				for (int k = 0; k < randomAmount; k++) {
					CreateMonster(SUMMON_TYPE.Wolf, landmark.tileLocation.settlementOnTile, landmark, structure);	
				}
			}
			//kobolds at ancient ruin
			List<BaseLandmark> ancientRuins = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.ANCIENT_RUIN);
			for (int i = 0; i < ancientRuins.Count; i++) {
				BaseLandmark landmark = ancientRuins[i];
				LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
				int randomAmount = 3;
				for (int k = 0; k < randomAmount; k++) {
					CreateMonster(SUMMON_TYPE.Kobold, landmark.tileLocation.settlementOnTile, landmark, structure);	
				}
			}
		}
		else {
			List<BaseLandmark> allLandmarks = LandmarkManager.Instance.GetAllLandmarks();
			for (int i = 0; i < allLandmarks.Count; i++) {
				BaseLandmark landmark = allLandmarks[i];
				if (landmark.specificLandmarkType != LANDMARK_TYPE.CAVE) {
					LocationStructure structure = landmark.tileLocation.GetMostImportantStructureOnTile();
					LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmark.specificLandmarkType);
					if (landmarkData.monsterGenerationSetting != null) {
						List<MonsterSetting> monsterChoices = landmarkData.monsterGenerationSetting.
							GetMonsterChoicesForBiome(landmark.tileLocation.biomeType);
						if (monsterChoices != null) {
							int iterations = landmarkData.monsterGenerationSetting.iterations.Random();
							for (int j = 0; j < iterations; j++) {
								MonsterSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(monsterChoices);
								int randomAmount = randomMonsterSetting.minMaxRange.Random();
								for (int k = 0; k < randomAmount; k++) {
									CreateMonster(randomMonsterSetting.monsterType, landmark.tileLocation.settlementOnTile, landmark, structure);	
								}
							}
							yield return null;
						}
					}
				}
			}	
		}
	}
	private IEnumerator CaveMonsterGeneration() {
		LandmarkData caveData = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.CAVE);
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
				List<LocationStructure> caves = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
				caves = caves.OrderByDescending(x => x.tiles.Count).ToList();
				if (WorldConfigManager.Instance.isDemoWorld) {
					bool hasSpawnedSpiders = false;
					bool hasSpawnedGolems = false;
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						if (hasSpawnedSpiders == false) {
							hasSpawnedSpiders = true;
							//Giant spiders	
							int randomGiantSpider = Random.Range(2, 5);
							for (int k = 0; k < randomGiantSpider; k++) {
								CreateMonster(SUMMON_TYPE.Giant_Spider, cave.unoccupiedTiles.ToList(), cave, hexTilesOfCave.ToArray());
							}
							//Small spiders	
							int randomSmallSpider = Random.Range(3, 8);
							for (int k = 0; k < randomSmallSpider; k++) {
								CreateMonster(SUMMON_TYPE.Small_Spider, cave.unoccupiedTiles.ToList(), cave, hexTilesOfCave.ToArray());
							}
						} else if (hasSpawnedGolems == false) {
							hasSpawnedGolems = true;
							//Golem	
							int randomGolem = Random.Range(1, 3);
							for (int k = 0; k < randomGolem; k++) {
								CreateMonster(SUMMON_TYPE.Golem, cave.unoccupiedTiles.ToList(), cave, hexTilesOfCave.ToArray());
							}
							//Abomination	
							int randomAbomination = Random.Range(1, 3);
							for (int k = 0; k < randomAbomination; k++) {
								CreateMonster(SUMMON_TYPE.Abomination, cave.unoccupiedTiles.ToList(), cave, hexTilesOfCave.ToArray());
							}
						}
						if (hasSpawnedGolems && hasSpawnedSpiders) {
							break;
						}
					}
				}
				else {
					List<MonsterSetting> monsterChoices = caveData.monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
					for (int j = 0; j < caves.Count; j++) {
						LocationStructure cave = caves[j];
						List<HexTile> hexTilesOfCave = GetHexTileCountOfCave(cave);
						for (int k = 0; k < hexTilesOfCave.Count; k++) {
							MonsterSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(monsterChoices);
							int randomAmount = randomMonsterSetting.minMaxRange.Random();
							for (int l = 0; l < randomAmount; l++) {
								CreateMonster(randomMonsterSetting.monsterType, cave.unoccupiedTiles.ToList(), cave, hexTilesOfCave.ToArray());	
							}
						}
					}	
				}
			}
			yield return null;
		}
	}

	private List<HexTile> GetHexTileCountOfCave(LocationStructure caveStructure) {
		List<HexTile> tiles = new List<HexTile>();
		for (int i = 0; i < caveStructure.unoccupiedTiles.Count; i++) {
			LocationGridTile tile = caveStructure.unoccupiedTiles.ElementAt(i);
			if (tile.collectionOwner.isPartOfParentRegionMap && tiles.Contains(tile.collectionOwner.partOfHextile.hexTileOwner) == false) {
				tiles.Add(tile.collectionOwner.partOfHextile.hexTileOwner);
			}
		}
		return tiles;
	}
}
