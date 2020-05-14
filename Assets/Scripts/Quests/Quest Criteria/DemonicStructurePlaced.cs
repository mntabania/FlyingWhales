using Inner_Maps.Location_Structures;
namespace Quests {
    public class DemonicStructurePlaced : QuestCriteria {
        public override void Enable() {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        public override void Disable() {
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        
        private void OnStructurePlaced(LocationStructure structure) {
            if (structure is DemonicStructure) {
                SetCriteriaAsMet();
            }
        }
    }
}