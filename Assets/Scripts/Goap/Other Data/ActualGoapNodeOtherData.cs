using UnityEngine.Assertions;

public class ActualGoapNodeOtherData : OtherData {
    public ActualGoapNode action { get; }
    public override object obj => action;
    public ActualGoapNodeOtherData(ActualGoapNode action) {
        this.action = action;
    }
    public ActualGoapNodeOtherData(SaveDataActualGoapNodeOtherData action) {
        this.action = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(action.actionID);
    }
    public override SaveDataOtherData Save() {
        SaveDataActualGoapNodeOtherData saveDataActualGoapNodeOtherData = new SaveDataActualGoapNodeOtherData();
        saveDataActualGoapNodeOtherData.Save(this);
        return saveDataActualGoapNodeOtherData;
    }
}

#region Save Data
public class SaveDataActualGoapNodeOtherData : SaveDataOtherData {
    public string actionID;
    public override void Save(OtherData data) {
        base.Save(data);
        ActualGoapNodeOtherData actionData = data as ActualGoapNodeOtherData;
        Assert.IsNotNull(actionData);
        actionID = actionData.action.persistentID;
    }
    public override OtherData Load() {
        return new ActualGoapNodeOtherData(this);
    }
}
#endregion