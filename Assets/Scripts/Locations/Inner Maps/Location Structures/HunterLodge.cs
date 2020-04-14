namespace Inner_Maps.Location_Structures {
    public class HunterLodge : ManMadeStructure {
        public HunterLodge(Region location) : base(STRUCTURE_TYPE.HUNTER_LODGE, location) { }
        public HunterLodge(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}