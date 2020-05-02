using System;
using Inner_Maps.Location_Structures;
namespace Tutorial {
    public class DemonicStructurePlaced : TutorialQuestCriteria {
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