using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Spire : DemonicStructure {
        public Spire(Region location) : base(STRUCTURE_TYPE.SPIRE, location) {
            SetMaxHPAndReset(5000);
        }
        public Spire(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        public override string scenarioDescription => "The Spire allows the Player to spend Chaotic Energy to upgrade Spells, Afflictions and Abilities that they've already learned. Upgrades Powers usually also have higher Mana Cost so make sure you have enough Mana Pits before upgrading too much!";

        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.UPGRADE_ABILITIES);
        }
    }
}