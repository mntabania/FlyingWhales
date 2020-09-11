using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class LocationGridTileOtherData : OtherData {
    public LocationGridTile tile { get; }
    public override object obj => tile;
    
    public LocationGridTileOtherData(LocationGridTile tile) {
        this.tile = tile;
        if (tile == null) {
            Debug.LogWarning($"New LocationGridTileOtherData was created but provided tile was null, this is handled but weird.");
        }
    }
    public LocationGridTileOtherData(SaveDataLocationGridTileOtherData saveData) {
        if (saveData.tileID.hasValue) {
            tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData.tileID);    
        }
    }
    
    public override SaveDataOtherData Save() {
        SaveDataLocationGridTileOtherData saveDataLocationGridTileOtherData = new SaveDataLocationGridTileOtherData();
        saveDataLocationGridTileOtherData.Save(this);
        return saveDataLocationGridTileOtherData;
    }
}

#region Save Data
public class SaveDataLocationGridTileOtherData : SaveDataOtherData {
    public TileLocationSave tileID;
    public override void Save(OtherData data) {
        base.Save(data);
        LocationGridTileOtherData otherData = data as LocationGridTileOtherData;
        Assert.IsNotNull(otherData);
        if (otherData.tile != null) {
            //Need this because there are times that the provided tile can be null (i.e. Roam)
            tileID = new TileLocationSave(otherData.tile);    
        }
    }
    public override OtherData Load() {
        return new LocationGridTileOtherData(this);
    }
}
#endregion