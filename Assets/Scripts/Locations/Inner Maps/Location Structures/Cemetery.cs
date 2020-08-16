namespace Inner_Maps.Location_Structures {
    public class Cemetery : ManMadeStructure {
        public Cemetery(Region location) : base(STRUCTURE_TYPE.CEMETERY, location) {
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Cemetery(Region location, SaveDataLocationStructure data) : base(location, data) { 
            wallsAreMadeOf = RESOURCE.WOOD;
        }
    }
}