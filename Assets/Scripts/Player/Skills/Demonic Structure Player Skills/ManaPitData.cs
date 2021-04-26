using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class ManaPitData : DemonicStructurePlayerSkill {
    public override string name => "Mana Pit";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MANA_PIT;
    public override string description => "This Structure increases the player's maximum Mana capacity and hourly Mana regen.";
    public ManaPitData() {
        structureType = STRUCTURE_TYPE.MANA_PIT;
    }
    /*
    protected override string InvalidMessage(LocationGridTile tile) {
        if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.MANA_PIT)) {
            return "You can only have 1 Spire per region.";
        }
        return base.InvalidMessage(tile);
    }*/
}
