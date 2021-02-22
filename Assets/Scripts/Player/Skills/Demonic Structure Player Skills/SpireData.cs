using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class SpireData : DemonicStructurePlayerSkill {
    public override string name => "Spire";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SPIRE;
    public override string description => "This Structure let the player upgrade skills";
    public SpireData() {
        structureType = STRUCTURE_TYPE.SPIRE;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.SPIRE)) {
            return "You can only have 1 Spire per region.";
        }
        return base.InvalidMessage(tile);
    }
}
