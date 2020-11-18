namespace Inner_Maps.Location_Structures {
    public class Smithy : ManMadeStructure{
        public Smithy(Region location) : base(STRUCTURE_TYPE.SMITHY, location) {
            SetMaxHPAndReset(8000);
        }
        public Smithy(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}