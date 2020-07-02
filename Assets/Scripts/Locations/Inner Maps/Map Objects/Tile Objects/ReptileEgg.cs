public class ReptileEgg : MonsterEgg {

    public ReptileEgg() : base(TILE_OBJECT_TYPE.REPTILE_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(4)) { }

    #region Overrides
    public override string ToString() {
        return $"Reptile Egg {id.ToString()}";
    }
    #endregion

}