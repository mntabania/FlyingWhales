using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KennelData : DemonicStructurePlayerSkill {
    
    public override string name => "Kennel";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.KENNEL;
    public override string description => "This Structure allows the Player to imprison a monster. The Kennel will slowly breed it. The Player can summon these for various monster partie Imprisoned monsters may also be drained to produce Chaos Orbs.";
    public KennelData() {
        structureType = STRUCTURE_TYPE.KENNEL;
    }
}