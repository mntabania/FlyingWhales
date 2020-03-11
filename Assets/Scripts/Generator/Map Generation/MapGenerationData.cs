using System.Collections;
using System.Collections.Generic;
using System.IO;
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
	public static int WorldMapFeatureGenerationBatches = 200;
	public static int WorldMapHabitabilityGenerationBatches = 300;
	public static int InnerMapTileGenerationBatches = 400;
	public static int InnerMapSeamlessEdgeBatches = 200;
	public static int InnerMapDetailBatches = 200;
	public static int InnerMapElevationBatches = 200; 
	
	//world map
	public const float xOffset = 2.56f;
	public const float yOffset = 1.93f;
	public const int tileSize = 1;
	public int width;
	public int height;
	public int regionCount;
	public int[,] habitabilityValues;
	public BaseLandmark portal;
	public LocationStructure portalStructure;
	
	//family trees
	public FamilyTreeDatabase familyTreeDatabase;
	public Dictionary<RACE, List<FamilyTree>> familyTreesDictionary => familyTreeDatabase.allFamilyTreesDictionary;
	
	#region Family Trees
	public void InitializeFamilyTrees() {
		familyTreeDatabase = new FamilyTreeDatabase();
	}
	#endregion
}
