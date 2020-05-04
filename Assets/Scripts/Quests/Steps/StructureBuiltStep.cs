using Inner_Maps.Location_Structures;
namespace Quests.Steps {
    public class StructureBuiltStep : QuestStep {
        private readonly STRUCTURE_TYPE _neededStructure;
        
        public StructureBuiltStep(STRUCTURE_TYPE neededStructure, string stepDescription = "Build Structure") 
            : base(stepDescription) {
            _neededStructure = neededStructure;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(LocationStructure structure) {
            if (structure.structureType == _neededStructure) {
                Complete();
            }
        }
        #endregion
    }
}