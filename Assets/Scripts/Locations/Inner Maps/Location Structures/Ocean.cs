namespace Inner_Maps.Location_Structures {
    public class Ocean : NaturalStructure {
        public Ocean(Region location) : base(STRUCTURE_TYPE.OCEAN, location) { }
        public Ocean(Region location, SaveDataNaturalStructure data) : base(location, data) { }
        
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (occupiedArea != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(occupiedArea.gridTileComponent.centerGridTile.centeredWorldLocation);
            }
        }
        public override void ShowSelectorOnStructure() { }
        // public override void ShowSelectorOnStructure() {
        //     if (occupiedArea != null) {
        //         Selector.Instance.Select(occupiedArea);
        //     }
        // }
        protected override void OnTileAddedToStructure(LocationGridTile tile) {
            base.OnTileAddedToStructure(tile);
            tile.SetElevation(ELEVATION.WATER);
        }
    }
}