namespace Inner_Maps.Location_Structures {
    public class AbandonedMine : ManMadeStructure {
        public AbandonedMine(Region location) : base(STRUCTURE_TYPE.ABANDONED_MINE, location) {
            AddStructureTag(STRUCTURE_TAG.Dangerous);
            AddStructureTag(STRUCTURE_TAG.Resource);
            AddStructureTag(STRUCTURE_TAG.Monster_Spawner);
            AddStructureTag(STRUCTURE_TAG.Shelter);
        }
        public AbandonedMine(Region location, SaveDataManMadeStructure data) : base(location, data) {}
    }
}