using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Features;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TileFeatureGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating tile features...");
		yield return MapGenerator.Instance.StartCoroutine(GenerateFeaturesForAllTiles());
		if (WorldConfigManager.Instance.isDemoWorld) {
			DetermineSettlementsForDemo();
		}
		else {
			yield return MapGenerator.Instance.StartCoroutine(ComputeHabitabilityValues(data));
			if (IsGeneratedMapValid(data)) {
				DetermineSettlements(4, data);	
			} else {
				succeess = false;
			}
		}
		
	}
	private IEnumerator GenerateFeaturesForAllTiles() {
		List<HexTile> flatTilesWithNoFeatures = new List<HexTile>();
		int batchCount = 0;
		for (int x = 0; x < GridMap.Instance.width; x++) {
			for (int y = 0; y < GridMap.Instance.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				//only add features to tiles without features yet
				if (tile.elevationType == ELEVATION.TREES) {
					tile.featureComponent.AddFeature(TileFeatureDB.Wood_Source_Feature, tile);
				} else if (tile.elevationType == ELEVATION.MOUNTAIN) {
					tile.featureComponent.AddFeature(TileFeatureDB.Metal_Source_Feature, tile);	
				} else if (tile.elevationType == ELEVATION.PLAIN && tile.featureComponent.features.Count == 0) {
					flatTilesWithNoFeatures.Add(tile);	
				}
				batchCount++;
				if (batchCount >= MapGenerationData.WorldMapFeatureGenerationBatches) {
					batchCount = 0;
					yield return null;
				}
			}	
		}

		int stoneSourceCount = 5;
		int fertileCount = 8;
		int gameCount = 6;

		//stone source
		for (int i = 0; i < stoneSourceCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Stone_Source_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}		
		
		yield return null;
		
		//fertile
		for (int i = 0; i < fertileCount; i++) {
			if (flatTilesWithNoFeatures.Count <= 0) { break; }
			HexTile tile = CollectionUtilities.GetRandomElement(flatTilesWithNoFeatures);
			tile.featureComponent.AddFeature(TileFeatureDB.Fertile_Feature, tile);
			flatTilesWithNoFeatures.Remove(tile);
		}
		
		yield return null;
		if (WorldConfigManager.Instance.isDemoWorld) {
			//pigs
			HexTile pigTile = GridMap.Instance.map[2, 4];
			GameFeature pigGameFeature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
			pigGameFeature.SetSpawnType(SUMMON_TYPE.Pig);
			pigTile.featureComponent.AddFeature(pigGameFeature, pigTile);
			
			//sheep
			HexTile sheepTile = GridMap.Instance.map[4, 3];
			GameFeature sheepGameFeature = LandmarkManager.Instance.CreateTileFeature<GameFeature>(TileFeatureDB.Game_Feature);
			sheepGameFeature.SetSpawnType(SUMMON_TYPE.Sheep);
			sheepTile.featureComponent.AddFeature(sheepGameFeature, sheepTile);
		} else {
			List<HexTile> gameChoices = GridMap.Instance.normalHexTiles.Where(h =>
				h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES).ToList();
			for (int i = 0; i < gameCount; i++) {
				if (gameChoices.Count <= 0) { break; }
				HexTile tile = CollectionUtilities.GetRandomElement(gameChoices);
				tile.featureComponent.AddFeature(TileFeatureDB.Game_Feature, tile);
				gameChoices.Remove(tile);
			}	
		}
		
		
		//vents
		if (WorldConfigManager.Instance.isDemoWorld) {
			//add poison vent to 1 tile in demo build
			HexTile tile = GridMap.Instance.map[7, 5];
			tile.featureComponent.AddFeature(TileFeatureDB.Poison_Vent_Feature, tile);
		} else {
			List<HexTile> ventChoices = GridMap.Instance.normalHexTiles.Where(h => 
				h.featureComponent.HasFeature(TileFeatureDB.Poison_Vent_Feature) == false 
				&& h.featureComponent.HasFeature(TileFeatureDB.Vapor_Vent_Feature) == false
			).ToList();
			int ventFeatures = Random.Range(1, 6);
			for (int i = 0; i < ventFeatures; i++) {
				if (ventChoices.Count <= 0) { break; }
				HexTile tile = CollectionUtilities.GetRandomElement(ventChoices);
				tile.featureComponent.AddFeature(
					Random.Range(0, 2) == 0 ? TileFeatureDB.Poison_Vent_Feature : TileFeatureDB.Vapor_Vent_Feature, tile);
				ventChoices.Remove(tile);
			}	
		}
		
	}
	private IEnumerator ComputeHabitabilityValues(MapGenerationData data) {
		data.habitabilityValues = new int[data.width, data.height];
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				string summary = $"Computing habitability for {tile}";
				int habitability = 0;
				if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN || tile.elevationType == ELEVATION.TREES) {
					summary += "\n - 0 Elevation type is not plain";
					habitability = 0;
				} else {
					int adjacentWaterTiles = 0;
					int adjacentFlatTiles = 0;
					for (int i = 0; i < tile.AllNeighbours.Count; i++) {
						HexTile neighbour = tile.AllNeighbours[i];
						if (neighbour.elevationType == ELEVATION.PLAIN) {
							summary += "\n - +2 Has adjacent flat tile";
							habitability += 2;
							adjacentFlatTiles += 1;
						} else if (neighbour.elevationType == ELEVATION.WATER) {
							adjacentWaterTiles += 1;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Wood_Source_Feature)) {
							habitability += 3;
							summary += "\n - +3 Has adjacent wood source";
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Stone_Source_Feature)) {
							habitability += 3;
							summary += "\n - +3 Has adjacent stone source";
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Metal_Source_Feature)) {
							habitability += 4;
							summary += "\n - +4 Has adjacent metal source";
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
							habitability += 5;
							summary += "\n - +5 Has adjacent Fertile";
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Game_Feature)) {
							habitability += 4;
							summary += "\n - +4 Has adjacent Game";
						}
					}
					if (adjacentWaterTiles == 1) {
						habitability += 5;
						summary += "\n - +5 Has 1 adjacent water tile";
					}
					if (adjacentFlatTiles < 2) {
						habitability -= 10;
						summary += "\n - -10 Has less than 2 adjacent flat tiles.";
					}
				}
				data.habitabilityValues[x, y] = habitability;
				summary += $"\nTotal Habitability is {habitability.ToString()}";
				// Debug.Log(summary);
				batchCount++;
				if (batchCount >= MapGenerationData.WorldMapHabitabilityGenerationBatches) {
					batchCount = 0;
					yield return null;
				}
			}	
		}
	}
	private bool IsGeneratedMapValid(MapGenerationData data) {
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			bool hasHabitableTile = false;
			for (int j = 0; j < region.tiles.Count; j++) {
				HexTile tile = region.tiles[j];
				int habitabilityValue = data.habitabilityValues[tile.xCoordinate, tile.yCoordinate];
				if (habitabilityValue >= 10) {
					hasHabitableTile = true;
					break;
				}
			}
			if (hasHabitableTile == false) {
				Debug.LogWarning($"{region.name} has no habitable tiles");
				//current region has no habitable tile
				return false;
			}
		}
		return true;
	}
	
	private void DetermineSettlements(int count, MapGenerationData data) {
		List<Region> choices = new List<Region>(GridMap.Instance.allRegions);
		List<Region> settlementRegions = new List<Region>();

		Assert.IsTrue(choices.Count >= count,
			$"There are not enough regions for the number of settlements needed. Regions are {choices.Count.ToString()}. Needed settlements are {count.ToString()}");
		
		for (int i = 0; i < count; i++) {
			Region chosen = CollectionUtilities.GetRandomElement(choices);
			settlementRegions.Add(chosen);
			choices.Remove(chosen);
		}

		WeightedDictionary<int> tileCountWeights = new WeightedDictionary<int>();
		tileCountWeights.AddElement(2, 25);
		tileCountWeights.AddElement(3, 25);
		tileCountWeights.AddElement(4, 50);
		
		for (int i = 0; i < settlementRegions.Count; i++) {
			Region region = settlementRegions[i];
			HexTile habitableTile = GetTileWithHighestHabitability(region, data);
			Assert.IsNotNull(habitableTile, $"{region.name} could not find a habitable tile!");
			int tileCount = tileCountWeights.PickRandomElementGivenWeights();
			List<HexTile> chosenTiles = GetSettlementTiles(region, habitableTile, tileCount);
			for (int j = 0; j < chosenTiles.Count; j++) {
				HexTile settlementTile = chosenTiles[j];
				settlementTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, settlementTile);
				LandmarkManager.Instance.CreateNewLandmarkOnTile(settlementTile, LANDMARK_TYPE.VILLAGE);
			}
		}
	}
	private void DetermineSettlementsForDemo() {
		Region region = GridMap.Instance.allRegions[0];
		List<HexTile> chosenTiles = new List<HexTile> {
			GridMap.Instance.map[4, 5],
			GridMap.Instance.map[5, 5],
			GridMap.Instance.map[4, 6],
			GridMap.Instance.map[3, 5],
		};

		for (int i = 0; i < chosenTiles.Count; i++) {
			HexTile chosenTile = chosenTiles[i];
			chosenTile.SetElevation(ELEVATION.PLAIN);
			chosenTile.featureComponent.RemoveAllFeatures(chosenTile);
			chosenTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, chosenTile);
			LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenTile, LANDMARK_TYPE.VILLAGE);
		}
		
		List<HexTile> neighbouringTiles = GetNeighbouringTiles(chosenTiles);
		//if settlement is not adjacent to any water hex tile create one
		if (neighbouringTiles.Any(h => h.elevationType == ELEVATION.WATER) == false) {
			HexTile randomTile = CollectionUtilities.GetRandomElement(neighbouringTiles);
			randomTile.SetElevation(ELEVATION.WATER);
			randomTile.featureComponent.RemoveAllFeatures(randomTile);
		}
	}
	private List<HexTile> GetNeighbouringTiles(List<HexTile> tiles) {
		List<HexTile> neighbouringTiles = new List<HexTile>();
		for (int i = 0; i < tiles.Count; i++) {
			HexTile tile = tiles[i];
			for (int j = 0; j < tile.AllNeighbours.Count; j++) {
				HexTile neighbour = tile.AllNeighbours[j];
				if (tiles.Contains(neighbour) == false && neighbouringTiles.Contains(neighbour) == false) {
					neighbouringTiles.Add(neighbour);
				}
			}
		}
		return neighbouringTiles;
	}
	private HexTile GetTileWithHighestHabitability(Region region, MapGenerationData data) {
		int highestHabitability = 0;
		HexTile tileWithHighestHabitability = null;
		for (int i = 0; i < region.tiles.Count; i++) {
			HexTile tile = region.tiles[i];
			int habitability = data.habitabilityValues[tile.xCoordinate, tile.yCoordinate];
			if (habitability > highestHabitability) {
				tileWithHighestHabitability = tile;
				highestHabitability = habitability;
			}
		}
		return tileWithHighestHabitability;
	}
	private List<HexTile> GetSettlementTiles(Region region, HexTile startingTile, int tileCount) {
		List<HexTile> chosenTiles = new List<HexTile>(){startingTile};
		List<HexTile> choices = new List<HexTile>(startingTile.AllNeighbours.Where(h => h.region == region));

		while (chosenTiles.Count != tileCount) {
			HexTile chosenTile = null;
			List<HexTile> flatTileWithNoFeature = choices.Where(h =>
				h.elevationType == ELEVATION.PLAIN && h.featureComponent.features.Count == 0).ToList();
			if (flatTileWithNoFeature.Count > 0) {
				chosenTile = CollectionUtilities.GetRandomElement(flatTileWithNoFeature);
			} else {
				List<HexTile> treeTiles = choices.Where(h =>
					h.featureComponent.HasFeature(TileFeatureDB.Wood_Source_Feature) && h.featureComponent.features.Count == 1).ToList();
				if (treeTiles.Count > 0) {
					chosenTile = CollectionUtilities.GetRandomElement(treeTiles);
				} else {
					List<HexTile> flatOrTreeTiles = choices.Where(h => h.elevationType == ELEVATION.PLAIN 
					                                                   || h.elevationType == ELEVATION.TREES).ToList();
					if (flatOrTreeTiles.Count > 0) {
						chosenTile = CollectionUtilities.GetRandomElement(flatOrTreeTiles);
					}
				}
			}

			if (chosenTile == null) {
				break; //could not find any more tiles that meet the criteria
			} else {
				chosenTiles.Add(chosenTile);
				//add neighbours of chosen tile to choices, exclude tiles that have already been chosen and are already in the choices
				choices.AddRange(chosenTile.AllNeighbours.Where(n => choices.Contains(n) == false 
				                                                     && chosenTiles.Contains(n) == false && n.region == region));
			}
			
		}
		return chosenTiles;
	}
}
