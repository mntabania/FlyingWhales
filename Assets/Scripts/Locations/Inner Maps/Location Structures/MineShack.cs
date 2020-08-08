namespace Inner_Maps.Location_Structures {
    public class MineShack : ManMadeStructure {
        public MineShack(Region location) : base(STRUCTURE_TYPE.MINE_SHACK, location) {
            SetMaxHPAndReset(8000);
        }
        public MineShack(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}