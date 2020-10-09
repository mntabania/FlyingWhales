namespace Inner_Maps.Location_Structures {
    public class VampireCastle : ManMadeStructure {
        public VampireCastle(Region location) : base(STRUCTURE_TYPE.VAMPIRE_CASTLE, location) { }
        public VampireCastle(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}