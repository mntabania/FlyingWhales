using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Settlements;
using Locations.Tile_Features;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerSettlementGeneration : MapGenerationComponent {

	#region Random World
	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating landmarks...");
		RandomPlacePortal(data);
		yield return null;
	}
	private void RandomPlacePortal(MapGenerationData data) {
		List<HexTile> validPortalTiles;
		if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[1, 7]
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Oona) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[3, 1]
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Zenko) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[6, 4]
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pangat_Loo) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[3, 5]
			};
		} else if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Affatt) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[4, 4]
			};
		}  else {
			validPortalTiles = GridMap.Instance.normalHexTiles.Where(h =>
				(h.elevationType == ELEVATION.PLAIN || h.elevationType == ELEVATION.TREES)
				&& h.region.HasTileWithFeature(TileFeatureDB.Inhabited_Feature)
				&& HasSettlementNeighbour(h) == false 
				&& h.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature) == false
			).ToList();
		}
		
		Assert.IsTrue(validPortalTiles.Count > 0,
			"No valid portal tiles were found!");
		
		HexTile portalTile = CollectionUtilities.GetRandomElement(validPortalTiles);
		PlacePortal(portalTile, data);
	}
	private bool HasSettlementNeighbour(HexTile tile) {
		for (int i = 0; i < tile.AllNeighbours.Count; i++) {
			HexTile neighbour = tile.AllNeighbours[i];
			if (neighbour.featureComponent.HasFeature(TileFeatureDB.Inhabited_Feature)) {
				return true;
			}
		}
		return false;
	}
	#endregion

	#region Scneario Maps
	public override IEnumerator LoadScenarioData(MapGenerationData data, ScenarioMapData scenarioMapData) {
		SaveDataHextile saveDataHextile = scenarioMapData.worldMapSave.GetHexTileDataWithLandmark(LANDMARK_TYPE.THE_PORTAL);
		HexTile portalTile = GridMap.Instance.map[saveDataHextile.xCoordinate, saveDataHextile.yCoordinate];
		PlacePortal(portalTile, data);
		yield return null;
	}
	#endregion


	private void PlacePortal(HexTile portalTile, MapGenerationData data) {
		portalTile.SetElevation(ELEVATION.PLAIN);
		portalTile.featureComponent.RemoveAllFeatures(portalTile);
		BaseLandmark portalLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(portalTile, LANDMARK_TYPE.THE_PORTAL);
		PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(portalTile);
		playerSettlement.SetName("Demonic Intrusion");
		data.portal = portalTile;
	}
}
