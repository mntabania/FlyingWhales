namespace Inner_Maps.Location_Structures {
    public class MinerCamp : ManMadeStructure {
        public MinerCamp(Region location) : base(STRUCTURE_TYPE.MINER_CAMP, location) {
            SetMaxHPAndReset(8000);
        }
        public MinerCamp(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}