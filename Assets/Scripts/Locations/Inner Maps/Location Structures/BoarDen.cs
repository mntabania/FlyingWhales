namespace Inner_Maps.Location_Structures {
    public class BoarDen : AnimalDen {
        public BoarDen(Region location) : base(STRUCTURE_TYPE.BOAR_DEN, location) { }
        public BoarDen(Region location, SaveDataNaturalStructure data) : base(location, data) {}
    }
}