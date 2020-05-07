namespace Inner_Maps.Location_Structures {
    public class MageTower : ManMadeStructure {
        public MageTower(Region location) : base(STRUCTURE_TYPE.MAGE_TOWER, location) {
            AddStructureTag(STRUCTURE_TAG.Treasure);
            AddStructureTag(STRUCTURE_TAG.Magic_Power_Up);
            AddStructureTag(STRUCTURE_TAG.Counterattack);
            AddStructureTag(STRUCTURE_TAG.Shelter);
        }
        public MageTower(Region location, SaveDataLocationStructure data) : base(location, data) {
            AddStructureTag(STRUCTURE_TAG.Treasure);
            AddStructureTag(STRUCTURE_TAG.Magic_Power_Up);
            AddStructureTag(STRUCTURE_TAG.Counterattack);
            AddStructureTag(STRUCTURE_TAG.Shelter);
        }
    }
}