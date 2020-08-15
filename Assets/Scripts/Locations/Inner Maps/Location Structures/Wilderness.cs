namespace Inner_Maps.Location_Structures {
    public class Wilderness : NaturalStructure {
        public Wilderness(Region location) : base(STRUCTURE_TYPE.WILDERNESS, location) { }
        public Wilderness(Region location, SaveDataLocationStructure data) : base(location, data) { }
        
        public override void CenterOnStructure() {
            //cannot center on wilderness since it covers the whole map.
        }
    }
}