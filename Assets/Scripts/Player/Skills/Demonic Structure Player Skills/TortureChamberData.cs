using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChamberData : DemonicStructurePlayerSkill {
    public override string name => "Torture Chamber";
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE_CHAMBER;
    public override string description => "This Structure allows the player to torture Villagers. Seize a Villager and then place them on the Torture Chamber and use the Torture action to begin!";
    public TortureChamberData() {
        structureType = STRUCTURE_TYPE.TORTURE_CHAMBERS;
    }
}