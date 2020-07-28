namespace Quests {
    public class CharacterObtainedItem : QuestCriteria {
        private readonly TILE_OBJECT_TYPE _tileObjectType;
        public Character characterThatObtainedItem { get; private set; }
        public CharacterObtainedItem(TILE_OBJECT_TYPE tileObjectType) {
            _tileObjectType = tileObjectType;
        }
        
        public override void Enable() {
            Messenger.AddListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, OnCharacterObtainedItem);
        }
        public override void Disable() {
            Messenger.RemoveListener<TileObject, Character>(Signals.CHARACTER_OBTAINED_ITEM, OnCharacterObtainedItem);
        }

        private void OnCharacterObtainedItem(TileObject tileObject, Character character) {
            if (tileObject.tileObjectType == _tileObjectType) {
                characterThatObtainedItem = character;
                SetCriteriaAsMet();
            }
        }
    }
}