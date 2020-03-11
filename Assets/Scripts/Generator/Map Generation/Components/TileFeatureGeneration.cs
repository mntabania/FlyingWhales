using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class TileFeatureGeneration : MapGenerationComponent {

	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(GenerateFeaturesForAllTiles());
		yield return MapGenerator.Instance.StartCoroutine(ComputeHabitabilityValues(data));
		if (IsGeneratedMapValid(data)) {
			DetermineSettlements(4, data);	
		} else {
			succeess = false;
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

		List<HexTile> gameChoices = GridMap.Instance.normalHexTiles.Where(h =>
			h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES).ToList();
		yield return null;
		//game
		for (int i = 0; i < gameCount; i++) {
			if (gameChoices.Count <= 0) { break; }
			HexTile tile = CollectionUtilities.GetRandomElement(gameChoices);
			tile.featureComponent.AddFeature(TileFeatureDB.Game_Feature, tile);
			gameChoices.Remove(tile);
		}
	}
	private IEnumerator ComputeHabitabilityValues(MapGenerationData data) {
		data.habitabilityValues = new int[data.width, data.height];
		
		int batchCount = 0;
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				HexTile tile = GridMap.Instance.map[x, y];
				int habitability = 0;
				if (tile.elevationType == ELEVATION.WATER || tile.elevationType == ELEVATION.MOUNTAIN || tile.elevationType == ELEVATION.TREES) {
					habitability = 0;
				} else {
					int adjacentWaterTiles = 0;
					int adjacentFlatTiles = 0;
					for (int i = 0; i < tile.AllNeighbours.Count; i++) {
						HexTile neighbour = tile.AllNeighbours[i];
						if (neighbour.elevationType == ELEVATION.PLAIN) {
							habitability += 2;
							adjacentFlatTiles += 1;
						} else if (neighbour.elevationType == ELEVATION.WATER) {
							adjacentWaterTiles += 1;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Wood_Source_Feature)) {
							habitability += 3;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Stone_Source_Feature)) {
							habitability += 3;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Metal_Source_Feature)) {
							habitability += 4;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Fertile_Feature)) {
							habitability += 5;
						}
						if (neighbour.featureComponent.HasFeature(TileFeatureDB.Game_Feature)) {
							habitability += 4;
						}
					}
					if (adjacentWaterTiles == 1) {
						habitability += 5;
					}
					if (adjacentFlatTiles < 2) {
						habitability -= 10;
					}
				}
				data.habitabilityValues[x, y] = habitability;
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
		tileCountWeights.AddElement(1, 25);
		tileCountWeights.AddElement(2, 25);
		tileCountWeights.AddElement(3, 50);
		
		for (int i = 0; i < settlementRegions.Count; i++) {
			Region region = settlementRegions[i];
			HexTile habitableTile = GetTileWithHighestHabitability(region, data);
			Assert.IsNotNull(habitableTile, $"{region.name} could not find a habitable tile!");
			int tileCount = tileCountWeights.PickRandomElementGivenWeights();
			List<HexTile> chosenTiles = GetSettlementTiles(region, habitableTile, tileCount);
			for (int j = 0; j < chosenTiles.Count; j++) {
				HexTile settlementTile = chosenTiles[j];
				settlementTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, settlementTile);
				LandmarkManager.Instance.CreateNewLandmarkOnTile(settlementTile, LANDMARK_TYPE.VILLAGE, false);
			}
		}
		
		
		// string log = "Determining npcSettlement tiles";
		// List<Region> settlementChoices = new List<Region>(GridMap.Instance.allRegions);
		// for (int i = 0; i < count; i++) {
		// 	Region chosenRegion = CollectionUtilities.GetRandomElement(settlementChoices);
		// 	HexTile randomTile = CollectionUtilities.GetRandomElement(chosenRegion.tiles);
		// 	randomTile.SetElevation(ELEVATION.PLAIN);
		// 	randomTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, randomTile);
		// 	LandmarkManager.Instance.CreateNewLandmarkOnTile(randomTile, LANDMARK_TYPE.VILLAGE, false);
		// 	log += $"\nChose {randomTile.ToString()} to be a npcSettlement";
		// 	if (Random.Range(0, 2) == 1) {
		// 		//2 tiles are settlements
		// 		HexTile adjacentTile = CollectionUtilities.GetRandomElement(randomTile.AllNeighbours
		// 			.Where(x => x.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature) == false
		// 			            && x.region == randomTile.region).ToList());
		// 		adjacentTile.SetElevation(ELEVATION.PLAIN);
		// 		adjacentTile.featureComponent.AddFeature(TileFeatureDB.Inhabited_Feature, adjacentTile);
		// 		LandmarkManager.Instance.CreateNewLandmarkOnTile(adjacentTile, LANDMARK_TYPE.VILLAGE, false);
		// 		log += $"\nChose {adjacentTile.ToString()} next to {randomTile.ToString()} to be a npcSettlement";
		// 	}
		// 	settlementChoices.Remove(chosenRegion);
		// }
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
