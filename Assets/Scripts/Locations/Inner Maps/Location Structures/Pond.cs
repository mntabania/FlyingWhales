namespace Inner_Maps.Location_Structures {
    public class Pond : NaturalStructure {
        public Pond(Region location) : base(STRUCTURE_TYPE.POND, location) { }
        public Pond(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}