namespace Inner_Maps.Location_Structures {
    [System.Serializable]
    public struct StructureSetting {
        public RESOURCE resource;
        public STRUCTURE_TYPE structureType;

        public StructureSetting(STRUCTURE_TYPE structureType, RESOURCE resource) {
            this.structureType = structureType;
            this.resource = resource;
        }
    }
}