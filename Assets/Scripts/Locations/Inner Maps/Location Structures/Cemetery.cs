namespace Inner_Maps.Location_Structures {
    public class Cemetery : NaturalStructure {
        public Cemetery(Region location) : base(STRUCTURE_TYPE.CEMETERY, location) { }
        public Cemetery(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}