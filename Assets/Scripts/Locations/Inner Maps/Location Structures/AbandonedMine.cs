namespace Inner_Maps.Location_Structures {
    public class AbandonedMine : ManMadeStructure {
        public AbandonedMine(Region location) : base(STRUCTURE_TYPE.ABANDONED_MINE, location) { }
        public AbandonedMine(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}