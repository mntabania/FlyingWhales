public class WolfDen : AnimalBurrow {
    public WolfDen() : base(SUMMON_TYPE.Wolf){
        Initialize(TILE_OBJECT_TYPE.WOLF_DEN);
    }
    public WolfDen(SaveDataTileObject data) : base(data, SUMMON_TYPE.Wolf) {
        
    }
}