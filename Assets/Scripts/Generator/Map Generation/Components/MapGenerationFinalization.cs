using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class MapGenerationFinalization : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		yield return MapGenerator.Instance.StartCoroutine(FinalizeInnerMaps());
		yield return MapGenerator.Instance.StartCoroutine(ExecuteFeatureInitialActions());
		yield return MapGenerator.Instance.StartCoroutine(LoadItems());
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i]; 
			region.GenerateOuterBorders();
			region.HideBorders();
		}
		data.familyTreeDatabase.Save();
	}

	private IEnumerator FinalizeInnerMaps() {
		for (int i = 0; i < InnerMapManager.Instance.innerMaps.Count; i++) {
			InnerTileMap map = InnerMapManager.Instance.innerMaps[i];
			yield return MapGenerator.Instance.StartCoroutine(map.CreateSeamlessEdges());
			PathfindingManager.Instance.RescanGrid(map.pathfindingGraph);
			yield return null;
		}
	}

	private IEnumerator ExecuteFeatureInitialActions() {
		for (int i = 0; i < GridMap.Instance.normalHexTiles.Count; i++) {
			HexTile tile = GridMap.Instance.normalHexTiles[i];
			for (int j = 0; j < tile.featureComponent.features.Count; j++) {
				TileFeature feature = tile.featureComponent.features[j];
				feature.GameStartActions(tile);
			}
			yield return null;
		}
	}

	private IEnumerator LoadItems() {
		for (int i = 0; i < LandmarkManager.Instance.allSetttlements.Count; i++) {
			Settlement settlement = LandmarkManager.Instance.allSetttlements[i];
			if (settlement.locationType != LOCATION_TYPE.DUNGEON) {
				InnerMapManager.Instance.LoadInitialSettlementItems(settlement);
				yield return null;	
			}
		}
		for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
			Region region = GridMap.Instance.allRegions[i];
			LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
			for (int j = 0; j < 3; j++) {
				wilderness.AddPOI(
					InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MIMIC_TILE_OBJECT));	
			}
		}
	}
	
	
}
