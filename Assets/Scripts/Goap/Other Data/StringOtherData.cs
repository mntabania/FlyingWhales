using UnityEngine.Assertions;

public class StringOtherData : OtherData {
    public string str { get; }
    public override object obj => str;
    
    public StringOtherData(string str) {
        this.str = str;
    }
    public StringOtherData(SaveDataStringOtherData saveData) {
        this.str = saveData.str;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataStringOtherData saveDataIntOtherData = new SaveDataStringOtherData();
        saveDataIntOtherData.Save(this);
        return saveDataIntOtherData;
    }
}

#region Save Data
public class SaveDataStringOtherData : SaveDataOtherData {
    public string str;
    public override void Save(OtherData data) {
        base.Save(data);
        StringOtherData otherData = data as StringOtherData;
        Assert.IsNotNull(otherData);
        str = otherData.str;
    }
    public override OtherData Load() {
        return new StringOtherData(this);
    }
}
#endregion