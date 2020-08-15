using Locations.Settlements;

public class SaveDataPlayerSettlement : SaveDataBaseSettlement {
    public override BaseSettlement Load() {
        return LandmarkManager.Instance.LoadPlayerSettlement(this);
    }
}
