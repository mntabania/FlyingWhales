using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class ManaPit : DemonicStructure {

        public override string scenarioDescription => "The Mana Pit increases the player's maximum Mana capacity and hourly Mana regen.";
        public ManaPit(Region location) : base(STRUCTURE_TYPE.MANA_PIT, location) {
            SetMaxHPAndReset(1000);
        }
        public ManaPit(Region location, SaveDataDemonicStructure data) : base(location, data) { }
    }
}