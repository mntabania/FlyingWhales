using System;
using System.Collections.Generic;
using Inner_Maps;

public class SaveDataInnerMap : SaveData<InnerTileMap> {

    public List<SaveDataLocationGridTile> tileSaves;
    public override void Save(InnerTileMap innerTileMap) {
        tileSaves = new List<SaveDataLocationGridTile>();
        for (int x = 0; x < innerTileMap.width; x++) {
            for (int y = 0; y < innerTileMap.height; y++) {
                LocationGridTile locationGridTile = innerTileMap.map[x, y];
                SaveDataLocationGridTile saveDataLocationGridTile = new SaveDataLocationGridTile();
                saveDataLocationGridTile.Save(locationGridTile);
                tileSaves.Add(saveDataLocationGridTile);
            }    
        }
    }
}
