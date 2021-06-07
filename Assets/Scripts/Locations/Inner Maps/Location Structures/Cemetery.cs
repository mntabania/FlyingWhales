namespace Inner_Maps.Location_Structures {
    public class Cemetery : ManMadeStructure {
        public Cemetery(Region location) : base(STRUCTURE_TYPE.CEMETERY, location) {
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Cemetery(Region location, SaveDataManMadeStructure data) : base(location, data) { 
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        
        #region Damage
        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //cemeteries can be damaged  by any tile
            AdjustHP(amount, isPlayerSource: isPlayerSource);
            OnStructureDamaged();
        }
        #endregion
    }
}