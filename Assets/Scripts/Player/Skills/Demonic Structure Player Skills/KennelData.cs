using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KennelData : DemonicStructurePlayerSkill {
    
    public override string name => "Kennel";
    public override SPELL_TYPE type => SPELL_TYPE.KENNEL;
    public override string description => 
        $"This Structure allows the player to store a monster for future use. " +
        $"Any monster stored within the Kennel after successfully invading a world can then be summoned on a future playthrough. " +
        $"You can store up to 3 monsters.";
    public KennelData() {
        structureType = STRUCTURE_TYPE.KENNEL;
    }
}