﻿using System;
using System.Collections.Generic;
using Inner_Maps;

public class SaveDataInnerMap : SaveData<InnerTileMap> {

    public float xSeed;
    public float ySeed;
    public int biomeSeed;
    public int elevationSeed;
    public float biomeTransitionXSeed;
    public float biomeTransitionYSeed;
    public Dictionary<Point, SaveDataLocationGridTile> tileSaves;
    
    public override void Save(InnerTileMap innerTileMap) {
        xSeed = innerTileMap.xSeed;
        ySeed = innerTileMap.ySeed;
        biomeSeed = innerTileMap.biomePerlinSettings.seed;
        elevationSeed = innerTileMap.perlinElevationSettings.seed;
        biomeTransitionXSeed = innerTileMap.biomeTransitionXSeed;
        biomeTransitionYSeed = innerTileMap.biomeTransitionYSeed;
        tileSaves = new Dictionary<Point, SaveDataLocationGridTile>();
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
