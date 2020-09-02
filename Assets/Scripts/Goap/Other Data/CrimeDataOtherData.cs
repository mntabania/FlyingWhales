public class CrimeDataOtherData : OtherData {
    public CrimeData crimeData { get; }
    public override object obj => crimeData;
    public CrimeDataOtherData(CrimeData crimeData) {
        this.crimeData = crimeData;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataCrimeDataOtherData saveDataCrimeDataOtherData = new SaveDataCrimeDataOtherData();
        saveDataCrimeDataOtherData.Save(this);
        return saveDataCrimeDataOtherData;
    }
}

#region Save Data
public class SaveDataCrimeDataOtherData : SaveDataOtherData { }
#endregion