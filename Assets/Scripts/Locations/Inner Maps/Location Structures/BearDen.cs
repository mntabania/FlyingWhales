namespace Inner_Maps.Location_Structures {
    public class BearDen : AnimalDen {
        public BearDen(Region location) : base(STRUCTURE_TYPE.BEAR_DEN, location) { }
        public BearDen(Region location, SaveDataNaturalStructure data) : base(location, data) {}
    }
}