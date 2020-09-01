public class CrimeableOtherData : OtherData {
    public ICrimeable crimeable { get; }
    public override object obj => crimeable;
    
    public CrimeableOtherData(ICrimeable crimeable) {
        this.crimeable = crimeable;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataCrimableOtherData saveDataCrimableOtherData = new SaveDataCrimableOtherData();
        saveDataCrimableOtherData.Save(this);
        return saveDataCrimableOtherData;
    }
}

#region Save Data
public class SaveDataCrimableOtherData : SaveDataOtherData { }
#endregion