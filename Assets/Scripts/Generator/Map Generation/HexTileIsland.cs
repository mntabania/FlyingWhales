using System.Collections.Generic;

public class HexTileIsland {
    public List<HexTile> tilesInIsland { get; }

    public HexTileIsland(HexTile tile) {
        tilesInIsland = new List<HexTile> {tile};
    }
    public void MergeIsland(HexTileIsland otherIsland) {
        tilesInIsland.AddRange(otherIsland.tilesInIsland);
        otherIsland.ClearIsland();
    }
    public void ClearIsland() {
        tilesInIsland.Clear();
    }
    public bool IsConnectedToThisIsland(HexTileIsland otherIsland) {
        for (int i = 0; i < tilesInIsland.Count; i++) {
            HexTile tileInIsland = tilesInIsland[i];
            for (int j = 0; j < tileInIsland.AllNeighbours.Count; j++) {
                HexTile neighbour = tileInIsland.AllNeighbours[j];
                if (otherIsland.tilesInIsland.Contains(neighbour)) {
                    return true;
                }	
            }
        }
        return false;
    }
}