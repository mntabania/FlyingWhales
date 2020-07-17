namespace Inner_Maps.Location_Structures {
    public class AncientGraveyard : ManMadeStructure{
        public AncientGraveyard(Region location) : base(STRUCTURE_TYPE.ANCIENT_GRAVEYARD, location) { }
        public AncientGraveyard(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}