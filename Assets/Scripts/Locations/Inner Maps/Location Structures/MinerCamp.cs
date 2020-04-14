namespace Inner_Maps.Location_Structures {
    public class MinerCamp : ManMadeStructure {
        public MinerCamp(Region location) : base(STRUCTURE_TYPE.MINER_CAMP, location) { }
        public MinerCamp(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}