using UnityEngine.Assertions;

public class FactionOtherData : OtherData {
    public Faction faction { get; }
    public override object obj => faction;
   
    public FactionOtherData(Faction faction) {
        this.faction = faction;
    }
    public FactionOtherData(SaveDataFactionOtherData faction) {
        this.faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(faction.factionID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataFactionOtherData saveDataFactionOtherData = new SaveDataFactionOtherData();
        saveDataFactionOtherData.Save(this);
        return saveDataFactionOtherData;
    }
}

#region Save Data
public class SaveDataFactionOtherData : SaveDataOtherData {
    public string factionID;
    public override void Save(OtherData data) {
        base.Save(data);
        FactionOtherData otherData = data as FactionOtherData;
        Assert.IsNotNull(otherData);
        factionID = otherData.faction.persistentID;
    }
    public override OtherData Load() {
        return new FactionOtherData(this);
    }
}
#endregion