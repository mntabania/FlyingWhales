using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexTileData {
    [Header("General Tile Details")]
    public int id;
    public string persistentID;
    public int xCoordinate;
    public int yCoordinate;
    public string tileName;

    [Space(10)]
    [Header("Biome Settings")]
    public float elevationNoise;
    public float moistureNoise;
    public float temperature;
    public BIOMES biomeType;
    public ELEVATION elevationType;
}

[System.Serializable]
public class AreaData {
    public int id;
    public string persistentID;
    public int xCoordinate;
    public int yCoordinate;
    public string areaName;
    
    #region getters
    public Vector2 position => new Vector2(xCoordinate, yCoordinate);
    #endregion
}
