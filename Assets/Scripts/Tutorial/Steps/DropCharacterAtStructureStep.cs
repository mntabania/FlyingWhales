using System;
namespace Tutorial {
    public class DropCharacterAtStructureStep : TutorialQuestStep {
        
        private readonly STRUCTURE_TYPE _dropAt;
        private readonly System.Type _neededType;
        
        public DropCharacterAtStructureStep(STRUCTURE_TYPE dropAt, System.Type neededType, 
            string stepDescription = "Drop character at structure", string tooltip = "") : base(stepDescription) {
            _dropAt = dropAt;
            _neededType = neededType;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }

        #region Listeners
        private void CheckCompletion(IPointOfInterest poi) {
            Character droppedCharacter = null;
            if (_neededType == typeof(Summon) && poi is Summon summon) {
                droppedCharacter = summon;
            } else if (_neededType == typeof(Character) && poi is Character character) {
                droppedCharacter = character;
            }
            if (droppedCharacter?.currentStructure != null && droppedCharacter.currentStructure.structureType == _dropAt) {
                Complete();
            }
        }
        #endregion
    }
}