public class GooseEgg : MonsterEgg {

    public GooseEgg() : base(TILE_OBJECT_TYPE.GOOSE_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(4)) { }

    #region Overrides
    public override string ToString() {
        return $"Goose Egg {id.ToString()}";
    }
    #endregion

}