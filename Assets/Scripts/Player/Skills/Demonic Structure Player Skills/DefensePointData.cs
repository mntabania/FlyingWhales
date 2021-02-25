using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class DefensePointData : DemonicStructurePlayerSkill {
    public override string name => "Defense Point";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.DEFENSE_POINT;
    public override string description => "Allows the player to summon monsters to defend an area. The player may summon up to 3 monsters per Defense Point. These monsters will patrol around the Defense Point and attack anyone that gets near.";
    public DefensePointData() {
        structureType = STRUCTURE_TYPE.DEFENSE_POINT;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.DEFENSE_POINT)) {
            return "You can only have 1 Defense Point per region.";
        }
        return base.InvalidMessage(tile);
    }
}
