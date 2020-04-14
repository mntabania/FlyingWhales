namespace Inner_Maps.Location_Structures {
    public class AncientRuin : ManMadeStructure{
        public AncientRuin(Region location) : base(STRUCTURE_TYPE.ANCIENT_RUIN, location) { }
        public AncientRuin(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}