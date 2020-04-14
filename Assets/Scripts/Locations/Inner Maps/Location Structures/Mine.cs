namespace Inner_Maps.Location_Structures {
    public class Mine : ManMadeStructure {
        public Mine(Region location) : base(STRUCTURE_TYPE.MINE, location) { }
        public Mine(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}