using UnityEngine.Assertions;

public class TileObjectOtherData : OtherData {
    public TileObject tileObject { get; }
    public override object obj => tileObject;
    
    public TileObjectOtherData(TileObject tileObject) {
        this.tileObject = tileObject;
        if (tileObject is GenericTileObject genericTileObject) {
            genericTileObject.gridTileLocation.SetIsDefault(false);
        }
    }
    public TileObjectOtherData(SaveDataTileObjectOtherData saveData) {
        this.tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(saveData.tileObjectID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataTileObjectOtherData saveDataTileObjectOtherData = new SaveDataTileObjectOtherData();
        saveDataTileObjectOtherData.Save(this);
        return saveDataTileObjectOtherData;
    }
}

#region Save Data
public class SaveDataTileObjectOtherData : SaveDataOtherData {
    public string tileObjectID;
    public override void Save(OtherData data) {
        base.Save(data);
        TileObjectOtherData otherData = data as TileObjectOtherData;
        Assert.IsNotNull(otherData);
        tileObjectID = otherData.tileObject.persistentID;
    }
    public override OtherData Load() {
        return new TileObjectOtherData(this);
    }
}
#endregion