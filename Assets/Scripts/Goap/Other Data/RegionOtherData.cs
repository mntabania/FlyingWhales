using UnityEngine.Assertions;

public class RegionOtherData : OtherData {
    public Region region { get; }
    public override object obj => region;
    
    public RegionOtherData(Region region) {
        this.region = region;
    }
    public RegionOtherData(SaveDataRegionOtherData saveData) {
        this.region = DatabaseManager.Instance.regionDatabase.mainRegion;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataRegionOtherData saveDataRegionOtherData = new SaveDataRegionOtherData();
        saveDataRegionOtherData.Save(this);
        return saveDataRegionOtherData;
    }
}

#region Save Data
public class SaveDataRegionOtherData : SaveDataOtherData {
    public string regionID;
    public override void Save(OtherData data) {
        base.Save(data);
        RegionOtherData otherData = data as RegionOtherData;
        Assert.IsNotNull(otherData);
        regionID = otherData.region.persistentID;
    }
    public override OtherData Load() {
        return new RegionOtherData(this);
    }
}
#endregion