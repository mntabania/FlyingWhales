using Locations.Settlements;
using UnityEngine.Assertions;

public class SettlementOtherData : OtherData {
    public BaseSettlement settlement { get; }
    public override object obj => settlement;
    
    public SettlementOtherData(BaseSettlement settlement) {
        this.settlement = settlement;
    }
    public SettlementOtherData(SaveDataSettlementOtherData saveData) {
        this.settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveData.settlementID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataSettlementOtherData saveDataSettlementOtherData = new SaveDataSettlementOtherData();
        saveDataSettlementOtherData.Save(this);
        return saveDataSettlementOtherData;
    }
}

#region Save Data
public class SaveDataSettlementOtherData : SaveDataOtherData {
    public string settlementID;
    public override void Save(OtherData data) {
        base.Save(data);
        SettlementOtherData otherData = data as SettlementOtherData;
        Assert.IsNotNull(otherData);
        settlementID = otherData.settlement.persistentID;
    }
    public override OtherData Load() {
        return new SettlementOtherData(this);
    }
}
#endregion