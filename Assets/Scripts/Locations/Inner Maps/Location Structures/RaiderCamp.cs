namespace Inner_Maps.Location_Structures {
    public class RaiderCamp : ManMadeStructure {
        public RaiderCamp(Region location) : base(STRUCTURE_TYPE.RAIDER_CAMP, location) { }
        public RaiderCamp(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}