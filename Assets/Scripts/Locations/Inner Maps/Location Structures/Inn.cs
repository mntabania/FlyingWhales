namespace Inner_Maps.Location_Structures {
    public class Inn : ManMadeStructure{
        public Inn(Region location) : base(STRUCTURE_TYPE.INN, location) { }
        public Inn(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}