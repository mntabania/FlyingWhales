namespace Inner_Maps.Location_Structures {
    public class WolfDen : AnimalDen {
        public WolfDen(Region location) : base(STRUCTURE_TYPE.WOLF_DEN, location) { }
        public WolfDen(Region location, SaveDataNaturalStructure data) : base(location, data) {}
        
    }
}