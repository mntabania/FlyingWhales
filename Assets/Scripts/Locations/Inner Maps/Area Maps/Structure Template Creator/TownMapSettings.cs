using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TownMapSettings {

    public Point size;
    public TileTemplateData[] groundTiles;
    public TileTemplateData[] groundWallTiles;
    public TileTemplateData[] structureTiles;
    public TileTemplateData[] objectTiles;
    public TileTemplateData[] detailTiles;
    public List<BuildingSpotData> buildSpots;

    public TownMapSettings() {
        buildSpots = new List<BuildingSpotData>();
    }

    public void LogInfo() {
        string info = $"Town Map Info: {size}";
        info += $"\nGround tiles: {groundTiles.Length}";
        info += $"\nStructure tiles: {structureTiles.Length}";
        info += $"\nObejct tiles: {objectTiles.Length}";
        info += $"\nDetail tiles: {detailTiles.Length}";
        //Debug.Log(info);
    }

}
