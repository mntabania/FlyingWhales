using Inner_Maps;

[System.Serializable]
public struct TileLocationSave {
    public bool hasValue;
    //public string regionPersistentID;
    public int xPos;
    public int yPos;

    public TileLocationSave(LocationGridTile locationGridTile) {
        hasValue = true;
        //regionPersistentID = locationGridTile.parentMap.region.persistentID;
        xPos = locationGridTile.localPlace.x;
        yPos = locationGridTile.localPlace.y;
    }
}
