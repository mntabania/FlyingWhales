using System.Collections.Generic;
public class HexTileDatabase {
    
    public Dictionary<string, HexTile> hexTileByGUID { get; }
    public List<HexTile> allHexTiles { get; }

    public HexTileDatabase() {
        hexTileByGUID = new Dictionary<string, HexTile>();
        allHexTiles = new List<HexTile>();
    }

    public void RegisterHexTile(HexTile hexTile) {
        hexTileByGUID.Add(hexTile.data.persistentID, hexTile);
        allHexTiles.Add(hexTile);
    }
}
