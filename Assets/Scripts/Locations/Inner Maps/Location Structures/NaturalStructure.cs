namespace Inner_Maps.Location_Structures {
    public abstract class NaturalStructure : LocationStructure {
        public override System.Type serializedData => typeof(SaveDataNaturalStructure);

        protected NaturalStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        protected NaturalStructure(Region location, SaveDataNaturalStructure data) : base(location, data) { }
    }
}