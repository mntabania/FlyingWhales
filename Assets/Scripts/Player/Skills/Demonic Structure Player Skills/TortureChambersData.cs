using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChambersData : DemonicStructurePlayerSkill {
    public override string name => "Prison";
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE_CHAMBERS;
    public override string description => 
        $"This Structure allows the player to torture Villagers. " +
        $"Seize a Villager and then place them on the Torture Chamber and use the Torture action to begin!";
    public TortureChambersData() {
        structureType = STRUCTURE_TYPE.TORTURE_CHAMBERS;
    }
}