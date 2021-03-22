using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Spire : DemonicStructure {
        public Spire(Region location) : base(STRUCTURE_TYPE.SPIRE, location) { }
        public Spire(Region location, SaveDataDemonicStructure data) : base(location, data) { }
        
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_ABILITIES);
        }
    }
}