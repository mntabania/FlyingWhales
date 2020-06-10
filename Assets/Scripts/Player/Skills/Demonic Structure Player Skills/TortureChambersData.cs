using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChambersData : DemonicStructurePlayerSkill {
    public override string name => "Torture Chambers";
    public override SPELL_TYPE type => SPELL_TYPE.TORTURE_CHAMBERS;
    public override string description => 
        $"This Structure allows the player to torture {UtilityScripts.Utilities.VillagerIcon()}Villagers. " +
        $"Seize a {UtilityScripts.Utilities.VillagerIcon()}Villager and then place them on the Torture Chamber and use the Torture action to begin!";
    public TortureChambersData() {
        structureType = STRUCTURE_TYPE.TORTURE_CHAMBERS;
    }
}