using Inner_Maps;
using UnityEngine.Assertions;

public class LocationGridTileOtherData : OtherData {
    public LocationGridTile tile { get; }
    public override object obj => tile;
    
    public LocationGridTileOtherData(LocationGridTile tile) {
        this.tile = tile;
    }
    public LocationGridTileOtherData(SaveDataLocationGridTileOtherData saveData) {
        tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileByPersistentID(saveData.tileID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataLocationGridTileOtherData saveDataLocationGridTileOtherData = new SaveDataLocationGridTileOtherData();
        saveDataLocationGridTileOtherData.Save(this);
        return saveDataLocationGridTileOtherData;
    }
}

#region Save Data
public class SaveDataLocationGridTileOtherData : SaveDataOtherData {
    public string tileID;
    public override void Save(OtherData data) {
        base.Save(data);
        LocationGridTileOtherData otherData = data as LocationGridTileOtherData;
        Assert.IsNotNull(otherData);
        tileID = otherData.tile.persistentID;
    }
    public override OtherData Load() {
        return new LocationGridTileOtherData(this);
    }
}
#endregion