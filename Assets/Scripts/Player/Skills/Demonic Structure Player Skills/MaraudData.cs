using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MaraudData : DemonicStructurePlayerSkill {
    public override string name => "Maraud";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MARAUD;
    public override string description => "Allows the player to form a monster raid party that he can then instruct to raid a target village. A monster raid party can have up to 3 monsters.";
    public MaraudData() {
        structureType = STRUCTURE_TYPE.MARAUD;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.MARAUD)) {
            return "You can only have 1 Maraud per region.";
        }
        return base.InvalidMessage(tile);
    }
}
