public class RabbitHole : AnimalBurrow {
    public RabbitHole() : base(SUMMON_TYPE.Rabbit){
        Initialize(TILE_OBJECT_TYPE.RABBIT_HOLE);
    }
    public RabbitHole(SaveDataTileObject data) : base(data, SUMMON_TYPE.Rabbit) {
        
    }
}