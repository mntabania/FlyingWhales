namespace Inner_Maps.Location_Structures {
    public class MooncrawlerHole : AnimalDen {
        public MooncrawlerHole(Region location) : base(STRUCTURE_TYPE.MOONCRAWLER_HOLE, location) { }
        public MooncrawlerHole(Region location, SaveDataNaturalStructure data) : base(location, data) {}
    }
}