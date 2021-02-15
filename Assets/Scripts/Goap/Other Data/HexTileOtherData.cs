using UnityEngine.Assertions;

public class AreaOtherData : OtherData {
    public Area area { get; }
    public override object obj => area;
    
    public AreaOtherData(Area p_area) {
        this.area = p_area;
    }
    public AreaOtherData(SaveDataAreaOtherData p_area) {
        this.area = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(p_area.areaID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataAreaOtherData saveDataAreaOtherData = new SaveDataAreaOtherData();
        saveDataAreaOtherData.Save(this);
        return saveDataAreaOtherData;
    }
}

#region Save Data
public class SaveDataAreaOtherData : SaveDataOtherData {
    public string areaID;
    public override void Save(OtherData data) {
        base.Save(data);
        AreaOtherData otherData = data as AreaOtherData;
        Assert.IsNotNull(otherData);
        areaID = otherData.area.persistentID;
    }
    public override OtherData Load() {
        return new AreaOtherData(this);
    }
}
#endregion