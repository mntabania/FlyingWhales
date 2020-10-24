namespace Inner_Maps.Location_Structures {
    [System.Serializable]
    public struct StructureSetting {
        public RESOURCE resource;
        public STRUCTURE_TYPE structureType;
        public bool hasValue;
        public bool isCorrupted;
        
        public StructureSetting(STRUCTURE_TYPE structureType, RESOURCE resource, bool isCorrupted = false) {
            this.structureType = structureType;
            this.resource = resource;
            this.isCorrupted = isCorrupted;
            hasValue = true;
        }
        public override string ToString() {
            return $"{resource.ToString()} {structureType.ToString()}";
        }

        #region Equality Members
        public bool Equals(StructureSetting other) {
            return resource == other.resource && structureType == other.structureType && isCorrupted == other.isCorrupted;
        }
        public override bool Equals(object obj) {
            return obj is StructureSetting other && Equals(other);
        }
        public override int GetHashCode() {
            unchecked {
                return ((int) resource * 397) ^ (int) structureType;
            }
        }
        #endregion
    }
}