namespace Quests {
    public class TileObjectActivated : QuestCriteria {
        private readonly TILE_OBJECT_TYPE _tileObjectType;
        
        public TileObject activatedObject { get; private set; }
        
        public TileObjectActivated(TILE_OBJECT_TYPE tileObjectType) {
            _tileObjectType = tileObjectType;
        }
        
        public override void Enable() {
            Messenger.AddListener<TileObject>(Signals.TILE_OBJECT_ACTIVATED, OnTileObjectActivated);
        }
        public override void Disable() {
            Messenger.RemoveListener<TileObject>(Signals.TILE_OBJECT_ACTIVATED, OnTileObjectActivated);
        }
        
        private void OnTileObjectActivated(TileObject tileObject) {
            if (tileObject.tileObjectType == _tileObjectType) {
                activatedObject = tileObject;
                SetCriteriaAsMet();
            }
        }
    }
}