public abstract class OtherData {
    public abstract object obj { get; }

    public abstract SaveDataOtherData Save();
}

#region Save Data
public abstract class SaveDataOtherData : SaveData<OtherData> { }
#endregion