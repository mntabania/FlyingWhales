using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class HarpyEgg : MonsterEgg {

    public override System.Type serializedData => typeof(SaveDataHarpyEgg);

    public HarpyEgg() : base(TILE_OBJECT_TYPE.HARPY_EGG, SUMMON_TYPE.Harpy, GameManager.Instance.GetTicksBasedOnHour(1)) { }
    public HarpyEgg(SaveDataHarpyEgg data) : base(data) { }
    
    #region Overrides
    public override string ToString() {
        return $"Harpy Egg {id.ToString()}";
    }
    #endregion

}
public class SaveDataHarpyEgg : SaveDataMonsterEgg { }