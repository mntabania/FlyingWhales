namespace Inner_Maps.Location_Structures {
    public class Apothecary : ManMadeStructure {
        public Apothecary(Region location) : base(STRUCTURE_TYPE.APOTHECARY, location) {
            SetMaxHPAndReset(8000);
        }
        public Apothecary(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}