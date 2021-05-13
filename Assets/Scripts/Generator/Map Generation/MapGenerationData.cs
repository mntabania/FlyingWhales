using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

/// <summary>
/// Class used to store all data for map generation, this data will be passed around between
/// map generation components.
/// </summary>
public class MapGenerationData {

	//batching values
	public static int WorldMapTileGenerationBatches = 200;
	public static int WorldMapOuterGridGenerationBatches = 200;
	public static int WorldMapElevationRefinementBatches = 400;
	public static int WorldMapFeatureGenerationBatches = 200;
	public static int WorldMapHabitabilityGenerationBatches = 300;
	public static int InnerMapTileGenerationBatches = 200;
	public static int InnerMapSeamlessEdgeBatches = 200;
	public static int InnerMapDetailBatches = 200;
	public static int InnerMapElevationBatches = 200;
	public static int TileObjectLoadingBatches = 300;
	public static int JobLoadingBatches = 300;  
	public static int LocationGridTileSecondaryWaveBatches = 300;
	
	//constants
	public const float XOffset = 2.56f;
	public const float YOffset = 1.93f;
	
	//world map
	public WorldMapTemplate chosenWorldMapTemplate;
	
	public List<VillageSpot> villageSpots { get; private set; }
	public List<Area> unreservedAreas { get; private set; }
	public List<Area> reservedAreas { get; private set; }
	public Dictionary<FactionTemplate, List<VillageSpot>> determinedVillages { get; private set; }
	public Dictionary<Area, List<LocationGridTile>> oceanBorderTilesCategorizedByArea { get; private set; }
	public Dictionary<Area, List<LocationGridTile>> caveBorderTilesCategorizedByArea { get; private set; }

	#region getters
	public int width => chosenWorldMapTemplate.worldMapWidth;
	public int height => chosenWorldMapTemplate.worldMapHeight;
	public int regionCount => chosenWorldMapTemplate.regionCount;
	#endregion

	public MapGenerationData() {
		villageSpots = new List<VillageSpot>();
		determinedVillages = new Dictionary<FactionTemplate, List<VillageSpot>>();
		oceanBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
		caveBorderTilesCategorizedByArea = new Dictionary<Area, List<LocationGridTile>>();
	}

	#region Village Spots
	public VillageSpot AddVillageSpot(Area p_villageSpot, List<Area> p_areas) {
		VillageSpot villageSpot = new VillageSpot(p_villageSpot, p_areas);
		villageSpots.Add(villageSpot);
		return villageSpot;
	}
	public void RemoveVillageSpot(VillageSpot p_villageSpot) {
		villageSpots.Remove(p_villageSpot);
	}
	public void AddDeterminedVillage(FactionTemplate p_faction, VillageSpot p_spot) {
		if (!determinedVillages.ContainsKey(p_faction)) {
			determinedVillages.Add(p_faction, new List<VillageSpot>());
		}
		determinedVillages[p_faction].Add(p_spot);
		Debug.Log($"Determined Village for {p_faction.name}: {p_spot.ToString()}");
	}
	public void SetUnreservedAreas(List<Area> p_areas) {
		unreservedAreas = p_areas;
	}
	public void SetReservedAreas(List<Area> p_areas) {
		reservedAreas = p_areas;
	}
	#endregion

	#region Border Tiles
	public void AddOceanBorderTile(Area p_area, LocationGridTile p_tile) {
		if (!oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			oceanBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		oceanBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}
	public void RemoveOceanBorderTile(Area p_area, LocationGridTile p_tile) {
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			oceanBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (oceanBorderTilesCategorizedByArea[p_area].Count == 0) {
				oceanBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}
	public LocationGridTile GetFirstUnoccupiedNonEdgeOceanTile(Area p_area) {
		if (oceanBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			List<LocationGridTile> tiles = oceanBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < tiles.Count; i++) {
				LocationGridTile tile = tiles[i];
				if (tile.tileObjectComponent.objHere == null && !tile.IsAtEdgeOfMap()) {
					return tile;
				}
			}
		}
		return null;
	}
	public void AddCaveBorderTile(Area p_area, LocationGridTile p_tile) {
		if (!caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			caveBorderTilesCategorizedByArea.Add(p_area, RuinarchListPool<LocationGridTile>.Claim());
		}
		caveBorderTilesCategorizedByArea[p_area].Add(p_tile);
	}
	public void RemoveCaveBorderTile(Area p_area, LocationGridTile p_tile) {
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			caveBorderTilesCategorizedByArea[p_area].Remove(p_tile);
			if (caveBorderTilesCategorizedByArea[p_area].Count == 0) {
				caveBorderTilesCategorizedByArea.Remove(p_area);
			}
		}
	}
	public LocationGridTile GetFirstUnoccupiedNonEdgeCaveTile(Area p_area) {
		if (caveBorderTilesCategorizedByArea.ContainsKey(p_area)) {
			List<LocationGridTile> tiles = caveBorderTilesCategorizedByArea[p_area];
			for (int i = 0; i < tiles.Count; i++) {
				LocationGridTile tile = tiles[i];
				if (tile.tileObjectComponent.objHere is BlockWall && !tile.IsAtEdgeOfMap()) {
					return tile;
				}
			}
		}
		return null;
	}
	#endregion

	#region Clean Up
	public void CleanUpAfterMapGeneration() {
		foreach (var kvp in oceanBorderTilesCategorizedByArea) {
			RuinarchListPool<LocationGridTile>.Release(kvp.Value);
		}
		foreach (var kvp in caveBorderTilesCategorizedByArea) {
			RuinarchListPool<LocationGridTile>.Release(kvp.Value);
		}
		oceanBorderTilesCategorizedByArea.Clear();
		caveBorderTilesCategorizedByArea.Clear();
		oceanBorderTilesCategorizedByArea = null;
		caveBorderTilesCategorizedByArea = null;
	}
	#endregion
}