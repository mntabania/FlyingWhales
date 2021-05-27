public class MinkHole : AnimalBurrow {
    public MinkHole() : base(SUMMON_TYPE.Mink){
        Initialize(TILE_OBJECT_TYPE.MINK_HOLE);
    }
    public MinkHole(SaveDataTileObject data) : base(data, SUMMON_TYPE.Mink) {
        
    }
}