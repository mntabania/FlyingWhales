namespace Quests {
    public class TileObjectActivated<T> : QuestCriteria where T : TileObject{

        public TileObject activatedObject { get; private set; }

        public override void Enable() {
            Messenger.AddListener<TileObject>(Signals.TILE_OBJECT_ACTIVATED, OnTileObjectActivated);
        }
        public override void Disable() {
            Messenger.RemoveListener<TileObject>(Signals.TILE_OBJECT_ACTIVATED, OnTileObjectActivated);
        }
        
        private void OnTileObjectActivated(TileObject tileObject) {
            if (tileObject is T) {
                activatedObject = tileObject;
                SetCriteriaAsMet();
            }
        }
    }
}