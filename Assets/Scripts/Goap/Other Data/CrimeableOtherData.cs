using UnityEngine.Assertions;

public class CrimeableOtherData : OtherData {
    public ICrimeable crimeable { get; }
    public override object obj => crimeable;
    
    public CrimeableOtherData(ICrimeable crimeable) {
        this.crimeable = crimeable;
    }
    public CrimeableOtherData(SaveDataCrimableOtherData data) {
        if (data.rumorableObjectType == OBJECT_TYPE.Action) {
            this.crimeable = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.rumorableID);    
        } else if (data.rumorableObjectType == OBJECT_TYPE.Interrupt) {
            this.crimeable = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.rumorableID);    
        }
    }
    
    public override SaveDataOtherData Save() {
        SaveDataCrimableOtherData saveDataCrimableOtherData = new SaveDataCrimableOtherData();
        saveDataCrimableOtherData.Save(this);
        return saveDataCrimableOtherData;
    }
}

#region Save Data
public class SaveDataCrimableOtherData : SaveDataOtherData {
    public string rumorableID;
    public OBJECT_TYPE rumorableObjectType;
    public override void Save(OtherData data) {
        base.Save(data);
        CrimeableOtherData otherData = data as CrimeableOtherData;
        Assert.IsNotNull(otherData);
        rumorableID = otherData.crimeable.persistentID;
        rumorableObjectType = otherData.crimeable.objectType;
    }
    public override OtherData Load() {
        return new CrimeableOtherData(this);
    }
    
}
#endregion