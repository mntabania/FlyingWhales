public class MooncrawlerHole : AnimalBurrow {
    public MooncrawlerHole() : base(SUMMON_TYPE.Moonwalker){
        Initialize(TILE_OBJECT_TYPE.MOONCRAWLER_HOLE);
    }
    public MooncrawlerHole(SaveDataTileObject data) : base(data, SUMMON_TYPE.Moonwalker) { }
}