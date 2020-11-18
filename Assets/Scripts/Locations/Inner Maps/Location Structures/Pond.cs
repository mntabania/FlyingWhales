namespace Inner_Maps.Location_Structures {
    public class Pond : NaturalStructure {
        public Pond(Region location) : base(STRUCTURE_TYPE.POND, location) { }
        public Pond(Region location, SaveDataNaturalStructure data) : base(location, data) { }
        
        public override void CenterOnStructure() {
            if (occupiedHexTile != null) {
                occupiedHexTile.hexTileOwner.CenterCameraHere();
            }
        }
    }
}