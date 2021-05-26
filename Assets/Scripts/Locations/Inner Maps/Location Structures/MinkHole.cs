namespace Inner_Maps.Location_Structures {
    public class MinkHole : AnimalDen {
        public MinkHole(Region location) : base(STRUCTURE_TYPE.MINK_HOLE, location) { }
        public MinkHole(Region location, SaveDataNaturalStructure data) : base(location, data) {}
    }
}