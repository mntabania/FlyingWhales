using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class StructureSettingOtherData : OtherData {
    public StructureSetting structureSetting { get; }
    public override object obj => structureSetting;
    
    public StructureSettingOtherData(StructureSetting structureSetting) {
        this.structureSetting = structureSetting;
    }
    public StructureSettingOtherData(SaveDataStructureSettingOtherData saveData) {
        this.structureSetting = saveData.structureSetting;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataStructureSettingOtherData saveData = new SaveDataStructureSettingOtherData();
        saveData.Save(this);
        return saveData;
    }
}

#region Save Data
public class SaveDataStructureSettingOtherData : SaveDataOtherData {
    public StructureSetting structureSetting;
    public override void Save(OtherData data) {
        base.Save(data);
        StructureSettingOtherData otherData = data as StructureSettingOtherData;
        Assert.IsNotNull(otherData);
        structureSetting = otherData.structureSetting;
    }
    public override OtherData Load() {
        return new StructureSettingOtherData(this);
    }
}
#endregion