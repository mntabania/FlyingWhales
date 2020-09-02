using UnityEngine.Assertions;

public class IntOtherData : OtherData {
    public int integer { get; }
    public override object obj => integer;
    
    public IntOtherData(int integer) {
        this.integer = integer;
    }
    public IntOtherData(SaveDataIntOtherData saveData) {
        this.integer = saveData.integer;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataIntOtherData saveDataIntOtherData = new SaveDataIntOtherData();
        saveDataIntOtherData.Save(this);
        return saveDataIntOtherData;
    }
}

#region Save Data
public class SaveDataIntOtherData : SaveDataOtherData {
    public int integer;
    public override void Save(OtherData data) {
        base.Save(data);
        IntOtherData otherData = data as IntOtherData;
        Assert.IsNotNull(otherData);
        integer = otherData.integer;
    }
    public override OtherData Load() {
        return new IntOtherData(this);
    }
}
#endregion