using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class PortalLandmarkGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		LevelLoaderManager.Instance.UpdateLoadingInfo("Generating landmarks...");
		PlacePortal(data);
		yield return null;
	}

	private void PlacePortal(MapGenerationData data) {
		List<HexTile> validPortalTiles;
		if (WorldConfigManager.Instance.isDemoWorld) {
			validPortalTiles = new List<HexTile>() {
				GridMap.Instance.map[1, 7]
			};
		} else {
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
		if (WorldConfigManager.Instance.isDemoWorld) {
			portalTile.SetElevation(ELEVATION.PLAIN);
		}
		portalTile.featureComponent.RemoveAllFeatures(portalTile);
		BaseLandmark portalLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(portalTile, LANDMARK_TYPE.THE_PORTAL);
		PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(portalTile);
		playerSettlement.SetName("Demonic Intrusion");
		data.portal = portalLandmark;
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
}
