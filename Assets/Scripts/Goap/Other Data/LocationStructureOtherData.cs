using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class LocationStructureOtherData : OtherData {
    public LocationStructure locationStructure { get; }
    public override object obj => locationStructure;
    
    public LocationStructureOtherData(LocationStructure locationStructure) {
        this.locationStructure = locationStructure;
    }
    public LocationStructureOtherData(SaveDataLocationStructureOtherData saveData) {
        this.locationStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveData.structureID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataLocationStructureOtherData saveDataLocationStructureOtherData = new SaveDataLocationStructureOtherData();
        saveDataLocationStructureOtherData.Save(this);
        return saveDataLocationStructureOtherData;
    }
}

#region Save Data
public class SaveDataLocationStructureOtherData : SaveDataOtherData {
    public string structureID;
    public override void Save(OtherData data) {
        base.Save(data);
        LocationStructureOtherData otherData = data as LocationStructureOtherData;
        Assert.IsNotNull(otherData);
        structureID = otherData.locationStructure.persistentID;
    }
    public override OtherData Load() {
        return new LocationStructureOtherData(this);
    }
}
#endregion