public class RumorOtherData : OtherData {
    public Rumor rumor { get; }
    public override object obj => rumor;
    
    public RumorOtherData(Rumor rumor) {
        this.rumor = rumor;
    }
    
    public override SaveDataOtherData Save() {
        SaveDataRumorOtherData saveDataRumorOtherData = new SaveDataRumorOtherData();
        saveDataRumorOtherData.Save(this);
        return saveDataRumorOtherData;
    }
}

#region Save Data
public class SaveDataRumorOtherData : SaveDataOtherData { }
#endregion