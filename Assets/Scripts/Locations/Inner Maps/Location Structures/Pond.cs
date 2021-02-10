namespace Inner_Maps.Location_Structures {
    public class Pond : NaturalStructure {
        public Pond(Region location) : base(STRUCTURE_TYPE.POND, location) { }
        public Pond(Region location, SaveDataNaturalStructure data) : base(location, data) { }
        
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (occupiedArea != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(occupiedArea.GetCenterLocationGridTile().centeredWorldLocation);
            }
        }
        public override void ShowSelectorOnStructure() {
            if (occupiedArea != null) {
                Selector.Instance.Select(occupiedArea);
            }
        }
    }
}