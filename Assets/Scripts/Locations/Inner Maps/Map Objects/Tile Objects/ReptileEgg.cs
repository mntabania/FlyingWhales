public class ReptileEgg : MonsterEgg {

    public override System.Type serializedData => typeof(SaveDataReptileEgg);

    public ReptileEgg() : base(TILE_OBJECT_TYPE.REPTILE_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(4)) { }
    public ReptileEgg(SaveDataReptileEgg data) : base(data) { }
    
    #region Overrides
    public override string ToString() {
        return $"Reptile Egg {id.ToString()}";
    }
    #endregion

}

public class SaveDataReptileEgg : SaveDataMonsterEgg { }