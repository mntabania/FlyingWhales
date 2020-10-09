using System.Diagnostics;
using UnityEngine.Assertions;
using Interrupts;

public class RumorOtherData : OtherData {
    public Rumor rumor { get; private set; }
    public override object obj => rumor;
    
    public RumorOtherData(Rumor rumor) {
        this.rumor = rumor;
    }
    public RumorOtherData(SaveDataRumorOtherData data) { }
    public override void LoadAdditionalData(SaveDataOtherData data) {
        base.LoadAdditionalData(data);
        //NOTE: Needed to move this here, because this depends on the data of all actions being loaded before it is assigned
        //otherwise, it could assign a null value, if the rumor data of the referenced action has not been loaded yet
        //TODO: It would be better to reference the IRumorable in this other data, but it would break the players current save data, so not doing it now.
        SaveDataRumorOtherData rumorOtherData = data as SaveDataRumorOtherData;
        Debug.Assert(rumorOtherData != null, nameof(rumorOtherData) + " != null");
        if (rumorOtherData.rumorableObjectType == OBJECT_TYPE.Action) {
            var action = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(rumorOtherData.rumorableID);
            Assert.IsNotNull(action.rumor, $"RumorOtherData has found action {action}, but its rumor data is null!");
            rumor = action.rumor;
        } else if (rumorOtherData.rumorableObjectType == OBJECT_TYPE.Interrupt) {
            var interrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(rumorOtherData.rumorableID);
            Assert.IsNotNull(interrupt.rumor, $"RumorOtherData has found action {interrupt}, but its rumor data is null!");
            rumor = interrupt.rumor;
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