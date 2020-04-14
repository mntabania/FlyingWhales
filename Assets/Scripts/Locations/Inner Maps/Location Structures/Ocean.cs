namespace Inner_Maps.Location_Structures {
    public class Ocean : NaturalStructure {
        public Ocean(Region location) : base(STRUCTURE_TYPE.OCEAN, location) { }
        public Ocean(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}