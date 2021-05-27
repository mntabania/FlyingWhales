public class BearDen : AnimalBurrow {
    public BearDen() : base(SUMMON_TYPE.Bear){
        Initialize(TILE_OBJECT_TYPE.BEAR_DEN);
    }
    public BearDen(SaveDataTileObject data) : base(data, SUMMON_TYPE.Bear) {
        
    }
}