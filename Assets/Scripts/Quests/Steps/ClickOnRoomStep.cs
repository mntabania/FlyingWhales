using System;
using Inner_Maps.Location_Structures;
namespace Quests.Steps {
    public class ClickOnRoomStep : QuestStep {
        private readonly Func<StructureRoom, bool> _validityChecker;
        public ClickOnRoomStep(string stepDescription = "Click on a room", 
            System.Func<StructureRoom, bool> validityChecker = null) : base(stepDescription) {
            _validityChecker = validityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is StructureRoom room) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(room)) {
                        Complete();
                    }
                } else {
                    Complete();    
                }
            }
        }
        #endregion
    }
}