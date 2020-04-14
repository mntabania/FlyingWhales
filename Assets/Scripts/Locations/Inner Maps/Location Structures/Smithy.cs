namespace Inner_Maps.Location_Structures {
    public class Smithy : ManMadeStructure{
        public Smithy(Region location) : base(STRUCTURE_TYPE.SMITHY, location) { }
        public Smithy(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}