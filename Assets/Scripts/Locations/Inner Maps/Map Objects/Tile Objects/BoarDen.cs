public class BoarDen : AnimalBurrow {
    public BoarDen() : base(SUMMON_TYPE.Boar){
        Initialize(TILE_OBJECT_TYPE.BOAR_DEN);
    }
    public BoarDen(SaveDataTileObject data) : base(data, SUMMON_TYPE.Boar) {
        
    }
}
