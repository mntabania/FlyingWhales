using Inner_Maps.Location_Structures;
namespace Quests {
    public class DemonicStructurePlaced : QuestCriteria {
        public override void Enable() {
            Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        public override void Disable() {
            Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        }
        
        private void OnStructurePlaced(LocationStructure structure) {
            if (structure is DemonicStructure) {
                SetCriteriaAsMet();
            }
        }
    }
}