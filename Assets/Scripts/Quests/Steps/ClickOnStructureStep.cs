using Inner_Maps.Location_Structures;
namespace Quests.Steps {
    public class ClickOnStructureStep : QuestStep {
        private readonly string _structureIdentifier;
        public ClickOnStructureStep(string stepDescription = "Click on a structure", 
            string structureIdentifier = "Normal") : base(stepDescription) {
            _structureIdentifier = structureIdentifier;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(ISelectable selectable) {
            if (selectable is LocationStructure) {
                if (_structureIdentifier == "Normal") {
                    Complete();    
                } else if (_structureIdentifier == "Demonic" && selectable is DemonicStructure) {
                    Complete();  
                }
                
            }
        }
        #endregion
    }
}