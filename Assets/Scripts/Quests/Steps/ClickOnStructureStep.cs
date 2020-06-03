using System;
using Inner_Maps.Location_Structures;
namespace Quests.Steps {
    public class ClickOnStructureStep : QuestStep {
        private readonly string _structureIdentifier;
        private readonly Func<LocationStructure, bool> _validityChecker;
        
        public ClickOnStructureStep(string stepDescription = "Click on a structure", 
            string structureIdentifier = "Normal", System.Func<LocationStructure, bool> validityChecker = null) : base(stepDescription) {
            _structureIdentifier = structureIdentifier;
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
            if (selectable is LocationStructure structure) {
                if (_validityChecker != null) {
                    if (_validityChecker.Invoke(structure)) {
                        Complete();
                    }
                } else {
                    if (_structureIdentifier == "Normal") {
                        Complete();    
                    } else if (_structureIdentifier == "Demonic" && selectable is DemonicStructure) {
                        Complete();  
                    }    
                }
            }
        }
        #endregion
    }
}