using System;
using System.Collections.Generic;
using Inner_Maps;
using Perlin_Noise;

public class SaveDataInnerMap : SaveData<InnerTileMap> {

    public float xSeed;
    public float ySeed;
    public PerlinNoiseSettings elevationPerlinNoiseSettings;
    public Dictionary<Point, SaveDataLocationGridTile> tileSaves;
    public float warpWeight;
    public float temperatureSeed;
    
    public override void Save(InnerTileMap innerTileMap) {
        xSeed = innerTileMap.xSeed;
        ySeed = innerTileMap.ySeed;
        elevationPerlinNoiseSettings = innerTileMap.elevationPerlinSettings;
        tileSaves = new Dictionary<Point, SaveDataLocationGridTile>();
        warpWeight = innerTileMap.warpWeight;
        temperatureSeed = innerTileMap.temperatureSeed;
        for (int x = 0; x < innerTileMap.width; x++) {
            for (int y = 0; y < innerTileMap.height; y++) {
                LocationGridTile locationGridTile = innerTileMap.map[x, y];
                if (locationGridTile.isDefault) {
                    continue; //skip
                }
                SaveDataLocationGridTile saveDataLocationGridTile = new SaveDataLocationGridTile();
                saveDataLocationGridTile.Save(locationGridTile);
                tileSaves.Add(new Point(x, y),  saveDataLocationGridTile);
            }    
        }
    }
    public SaveDataLocationGridTile GetSaveDataForTile(Point point) {
        if (tileSaves.ContainsKey(point)) {
            return tileSaves[point];
        }
        return null;
    }
}
