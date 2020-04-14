namespace Inner_Maps.Location_Structures {
    public class MageTower : ManMadeStructure {
        public MageTower(Region location) : base(STRUCTURE_TYPE.MAGE_TOWER, location) { }
        public MageTower(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}