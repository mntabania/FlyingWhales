using Inner_Maps.Location_Structures;
namespace Quests.Steps {
    public class DropCharacterAtStructureRoomStep<T> : QuestStep where T : StructureRoom{
        
        public DropCharacterAtStructureRoomStep(string stepDescription = "Drop character at Room") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }

        #region Listeners
        private void CheckCompletion(IPointOfInterest poi) {
            Character droppedCharacter = poi as Character;

            if (droppedCharacter?.gridTileLocation?.structure != null && 
                droppedCharacter.gridTileLocation.structure.IsTilePartOfARoom(droppedCharacter.gridTileLocation, out var room)) {
                if (room is T) {
                    Complete();
                }
            }
        }
        #endregion
    }
}