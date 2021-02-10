using UnityEngine.Assertions;

public class HexTileOtherData : OtherData {
    public HexTile hexTile { get; }
    public override object obj => hexTile;
    
    public HexTileOtherData(HexTile hexTile) {
        this.hexTile = hexTile;
    }
    public HexTileOtherData(SaveDataHexTileOtherData hexTile) {
        this.hexTile = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(hexTile.hexTileID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataHexTileOtherData saveDataHexTileOtherData = new SaveDataHexTileOtherData();
        saveDataHexTileOtherData.Save(this);
        return saveDataHexTileOtherData;
    }
}

#region Save Data
public class SaveDataHexTileOtherData : SaveDataOtherData {
    public string hexTileID;
    public override void Save(OtherData data) {
        base.Save(data);
        HexTileOtherData otherData = data as HexTileOtherData;
        Assert.IsNotNull(otherData);
        hexTileID = otherData.hexTile.persistentID;
    }
    public override OtherData Load() {
        return new HexTileOtherData(this);
    }
}
#endregion