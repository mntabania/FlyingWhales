using UnityEngine.Assertions;
using Interrupts;

public class RumorOtherData : OtherData {
    public Rumor rumor { get; }
    public override object obj => rumor;
    
    public RumorOtherData(Rumor rumor) {
        this.rumor = rumor;
    }
    public RumorOtherData(SaveDataRumorOtherData data) {
        if (data.rumorableObjectType == OBJECT_TYPE.Action) {
            this.rumor = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.rumorableID).rumor;    
        } else if (data.rumorableObjectType == OBJECT_TYPE.Interrupt) {
            this.rumor = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.rumorableID).rumor;    
        }
    }

    public override SaveDataOtherData Save() {
        SaveDataRumorOtherData saveDataRumorOtherData = new SaveDataRumorOtherData();
        saveDataRumorOtherData.Save(this);
        return saveDataRumorOtherData;
    }
}

#region Save Data
public class SaveDataRumorOtherData : SaveDataOtherData {
    public string rumorableID;
    public OBJECT_TYPE rumorableObjectType;
    public override void Save(OtherData data) {
        base.Save(data);
        RumorOtherData otherData = data as RumorOtherData;
        Assert.IsNotNull(otherData);
        rumorableID = otherData.rumor.rumorable.persistentID;
        rumorableObjectType = otherData.rumor.rumorable.objectType;

        if(otherData.rumor.rumorable is ActualGoapNode action) {
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(action);
        } else if (otherData.rumor.rumorable is InterruptHolder interrupt) {
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(interrupt);
        }
    }
    public override OtherData Load() {
        return new RumorOtherData(this);
    }
}
#endregion