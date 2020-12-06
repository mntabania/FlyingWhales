using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class EyeData : DemonicStructurePlayerSkill {
    public override string name => "Eye";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EYE;
    public override string description => "This Structure notifies the player of important events that occur within its region. Some of these events can then be stored as an Intel that can be shared to Villagers.";
    public EyeData() {
        structureType = STRUCTURE_TYPE.EYE;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.EYE)) {
            return "You can only have 1 Eye per region.";
        }
        return base.InvalidMessage(tile);
    }
}
