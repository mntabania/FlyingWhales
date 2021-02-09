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
            if (occupiedHexTile != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(occupiedHexTile.GetCenterLocationGridTile().centeredWorldLocation);
            }
        }
        public override void ShowSelectorOnStructure() {
            if (occupiedHexTile != null) {
                Selector.Instance.Select(occupiedHexTile);
            }
        }
    }
}