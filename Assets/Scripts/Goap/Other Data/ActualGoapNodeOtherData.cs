public class ActualGoapNodeOtherData : OtherData {
    public ActualGoapNode action { get; }
    public override object obj => action;
    public ActualGoapNodeOtherData(ActualGoapNode action) {
        this.action = action;
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
        //TODO: save action ID
    }
}
#endregion