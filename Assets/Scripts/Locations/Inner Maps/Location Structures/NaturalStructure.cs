namespace Inner_Maps.Location_Structures {
    public abstract class NaturalStructure : LocationStructure {
        protected NaturalStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        protected NaturalStructure(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}