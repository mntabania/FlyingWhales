public class GooseEgg : MonsterEgg {

    public override System.Type serializedData => typeof(SaveDataGooseEgg);

    public GooseEgg() : base(TILE_OBJECT_TYPE.GOOSE_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(4)) { }
    public GooseEgg(SaveDataGooseEgg data) : base(data) { }
    #region Overrides
    public override string ToString() {
        return $"Goose Egg {id.ToString()}";
    }
    #endregion

}

public class SaveDataGooseEgg : SaveDataMonsterEgg { }