public class SpiderEgg : MonsterEgg {

    public SpiderEgg() : base(TILE_OBJECT_TYPE.SPIDER_EGG, SUMMON_TYPE.Giant_Spider, GameManager.Instance.GetTicksBasedOnHour(4)) { }

    #region Overrides
    public override string ToString() {
        return $"Spider Egg {id.ToString()}";
    }
    #endregion
    
}
