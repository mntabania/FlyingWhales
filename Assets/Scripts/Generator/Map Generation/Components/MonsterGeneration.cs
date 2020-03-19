using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Assertions;
using UtilityScripts;

public class MonsterGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		// yield return MapGenerator.Instance.StartCoroutine(RegionalMonsterGeneration());
		yield return MapGenerator.Instance.StartCoroutine(LandmarkMonsterGeneration());
		// yield return MapGenerator.Instance.StartCoroutine(CaveMonsterGeneration());
		yield return null;
	}

	#region Helpers
	private void CreateMonster(SUMMON_TYPE summonType, BaseSettlement settlementOnTile, BaseLandmark monsterLair,
		LocationStructure monsterLairStructure) {
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, settlementOnTile, monsterLair.tileLocation.region);
		CharacterManager.Instance.PlaceSummon(summon, CollectionUtilities.GetRandomElement(monsterLairStructure.unoccupiedTiles));
		summon.AddTerritory(monsterLair.tileLocation);
		if (monsterLairStructure is IDwelling homeStructure) {
			summon.MigrateHomeStructureTo(homeStructure);	
		}
	}
	private void CreateMonster(SUMMON_TYPE summonType, List<LocationGridTile> locationChoices, LocationStructure homeStructure = null, params HexTile[] territories) {
		var chosenTile = homeStructure != null ? CollectionUtilities.GetRandomElement(homeStructure.unoccupiedTiles) : CollectionUtilities.GetRandomElement(locationChoices);
		Assert.IsTrue(chosenTile.collectionOwner.isPartOfParentRegionMap, $"Chosen tile for {summonType.ToString()} is not part of the region map!");
		Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, null, chosenTile.parentMap.region);
		CharacterManager.Instance.PlaceSummon(summon, chosenTile);
		summon.AddTerritory(chosenTile.collectionOwner.partOfHextile.hexTileOwner);
		if (territories != null) {
			for (int i = 0; i < territories.Length; i++) {
				HexTile territory = territories[i];
				summon.AddTerritory(territory);
			}
		}
		if (homeStructure is IDwelling structure) {
			summon.MigrateHomeStructureTo(structure);
		}
	}
	#endregion
	

	private IEnumerator RegionalMonsterGeneration() {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			List<LocationGridTile> locationChoices = new List<LocationGridTile>();
			region.tiles.Where(h => h.landmarkOnTile == null && h.elevationType == ELEVATION.PLAIN).ToList().
				ForEach(h => locationChoices.AddRange(h.locationGridTiles));
			MonsterGenerationSetting monsterGenerationSetting =
				WorldConfigManager.Instance.worldWideMonsterGenerationSetting;
			List<MonsterSetting> monsterChoices = monsterGenerationSetting.GetMonsterChoicesForBiome(region.coreTile.biomeType);
			if (monsterChoices != null) {
				int iterations = monsterGenerationSetting.iterations.Random();
				for (int j = 0; j < iterations; j++) {
					MonsterSetting randomMonsterSetting = CollectionUtilities.GetRandomElement(monsterChoices);
					int randomAmount = randomMonsterSetting.minMaxRange.Random();
					for (int k = 0; k < randomAmount; k++) {
						CreateMonster(randomMonsterSetting.monsterType, locationChoices);	
					}
				}	
			}
			yield return null;
		}
	}
	private IEnumerator LandmarkMonsterGeneration() {
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
	private IEnumerator CaveMonsterGeneration() {
		LandmarkData caveData = LandmarkManager.Instance.GetLandmarkData(LANDMARK_TYPE.CAVE);
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			if (region.HasStructure(STRUCTURE_TYPE.CAVE)) {
				List<LocationStructure> caves = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.CAVE);
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
