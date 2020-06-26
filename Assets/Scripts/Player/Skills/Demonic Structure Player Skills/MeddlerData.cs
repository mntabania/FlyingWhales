using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeddlerData : DemonicStructurePlayerSkill {
    
    public override string name => "Meddler";
    public override SPELL_TYPE type => SPELL_TYPE.MEDDLER;
    public override string description => "This Structure allows the player to trigger wars between factions. " +
                                          $"It can also be used to goad Villagers into leaving or joining a Faction of your choice.";
    public MeddlerData() {
        structureType = STRUCTURE_TYPE.MEDDLER;
    }
}
