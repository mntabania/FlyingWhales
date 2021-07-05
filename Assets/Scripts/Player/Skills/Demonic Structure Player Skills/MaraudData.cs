using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MaraudData : DemonicStructurePlayerSkill {
    public override string name => "Maraud";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MARAUD;
    public override string description => "This Structure allows the player to summon Raid Parties. Raid Parties harass Villages and is primarily used to generate some Chaos Orbs.";
    public MaraudData() {
        structureType = STRUCTURE_TYPE.MARAUD;
    }
}
