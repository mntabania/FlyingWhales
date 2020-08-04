using Inner_Maps.Location_Structures;
using Tutorial;
namespace Quests {
    public class StructureBuiltCriteria : QuestCriteria {
        private readonly STRUCTURE_TYPE _neededStructure;
        
        public StructureBuiltCriteria(STRUCTURE_TYPE neededStructure) {
            _neededStructure = neededStructure;
        }
        
        public override void Enable() {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, CheckCriteria);
        }
        public override void Disable() {
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, CheckCriteria);
        }
        
        
        private void CheckCriteria(LocationStructure structure) {
            if (structure.structureType == _neededStructure) {
                SetCriteriaAsMet();
            }
        }
    }
}