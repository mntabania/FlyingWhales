using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Inner_Maps.Location_Structures;
using UnityEngine;

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
	
	//world map
	public WorldMapTemplate chosenWorldMapTemplate;
	public const float XOffset = 2.56f;
	public const float YOffset = 1.93f;
	public const int TileSize = 1;
	public const int MinimumHabitabilityForVillage = 6;
	public int width => chosenWorldMapTemplate.worldMapWidth;
	public int height => chosenWorldMapTemplate.worldMapHeight;
	public int regionCount => chosenWorldMapTemplate.regionCount;
	public int[,] habitabilityValues;
	public List<HexTile> villageSpots;
	public Dictionary<FactionSetting, List<HexTile>> determinedVillages;
	public HexTile portal;

	public MapGenerationData() {
		villageSpots = new List<HexTile>();
		determinedVillages = new Dictionary<FactionSetting, List<HexTile>>();
	}
	
	#region Habitability
	public int GetHabitabilityValue(HexTile hexTile) {
		return habitabilityValues[hexTile.xCoordinate, hexTile.yCoordinate];
	}
	#endregion

	#region Village Spots
	public void AddVillageSpot(HexTile p_villageSpot) {
		if (!villageSpots.Contains(p_villageSpot)) {
			villageSpots.Add(p_villageSpot);
		}
	}
	public void RemoveVillageSpots(HexTile p_villageSpot) {
		villageSpots.Remove(p_villageSpot);
	}
	public void RemoveVillageSpots(List<HexTile> p_villageSpot) {
		for (int i = 0; i < p_villageSpot.Count; i++) {
			HexTile tile = p_villageSpot[i];
			RemoveVillageSpots(tile);
		}
	}
	public void AddDeterminedVillage(FactionSetting p_faction, HexTile p_tile) {
		if (!determinedVillages.ContainsKey(p_faction)) {
			determinedVillages.Add(p_faction, new List<HexTile>());
		}
		determinedVillages[p_faction].Add(p_tile);
	}
	#endregion
}
