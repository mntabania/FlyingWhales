namespace Inner_Maps.Location_Structures {
    public class Hospice : ManMadeStructure {
        
        public Hospice(Region location) : base(STRUCTURE_TYPE.HOSPICE, location) {
            SetMaxHPAndReset(8000);
        }
        public Hospice(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}