using UnityEngine.Assertions;

public class CrimeDataOtherData : OtherData {
    public CrimeData crimeData { get; }
    public override object obj => crimeData;
    public CrimeDataOtherData(CrimeData crimeData) {
        this.crimeData = crimeData;
    }
    public CrimeDataOtherData(SaveDataCrimeDataOtherData data) {
        this.crimeData = DatabaseManager.Instance.crimeDatabase.GetCrimeByPersistentID(data.crimeDataID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataCrimeDataOtherData saveDataCrimeDataOtherData = new SaveDataCrimeDataOtherData();
        saveDataCrimeDataOtherData.Save(this);
        return saveDataCrimeDataOtherData;
    }
}

#region Save Data
public class SaveDataCrimeDataOtherData : SaveDataOtherData {
    public string crimeDataID;
    public override void Save(OtherData data) {
        base.Save(data);
        CrimeDataOtherData crimeDataOtherData = data as CrimeDataOtherData;
        Assert.IsNotNull(crimeDataOtherData);
        crimeDataID = crimeDataOtherData.crimeData.persistentID;
    }
    public override OtherData Load() {
        return new CrimeDataOtherData(this);
    }
}
#endregion