using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Inner_Maps;
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
	public const int MinimumHabitabilityForVillage = 1;
	public int width => chosenWorldMapTemplate.worldMapWidth;
	public int height => chosenWorldMapTemplate.worldMapHeight;
	public int regionCount => chosenWorldMapTemplate.regionCount;
	public int[,] habitabilityValues;
	public List<Area> villageSpots;
	public Dictionary<FactionTemplate, List<Area>> determinedVillages;
	public Area portal;

	public MapGenerationData() {
		villageSpots = new List<Area>();
		determinedVillages = new Dictionary<FactionTemplate, List<Area>>();
	}
	
	#region Habitability
	public int GetHabitabilityValue(Area p_area) {
		return habitabilityValues[p_area.areaData.xCoordinate, p_area.areaData.yCoordinate];
	}
	#endregion

	#region Village Spots
	public void AddVillageSpot(Area p_villageSpot) {
		if (!villageSpots.Contains(p_villageSpot)) {
			villageSpots.Add(p_villageSpot);
		}
	}
	private void RemoveVillageSpots(Area p_villageSpot) {
		villageSpots.Remove(p_villageSpot);
	}
	public void RemoveVillageSpots(List<Area> p_villageSpot) {
		for (int i = 0; i < p_villageSpot.Count; i++) {
			Area tile = p_villageSpot[i];
			RemoveVillageSpots(tile);
		}
	}
	public void AddDeterminedVillage(FactionTemplate p_faction, Area p_area) {
		if (!determinedVillages.ContainsKey(p_faction)) {
			determinedVillages.Add(p_faction, new List<Area>());
		}
		determinedVillages[p_faction].Add(p_area);
		Debug.Log($"Determined Village for {p_faction.name}: {p_area.ToString()}");
	}
	#endregion
}
