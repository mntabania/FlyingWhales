public abstract class OtherData {
    public abstract object obj { get; }

    public abstract SaveDataOtherData Save();
    public virtual void LoadAdditionalData(SaveDataOtherData data) {}
}

#region Save Data
public abstract class SaveDataOtherData : SaveData<OtherData> { }
#endregion