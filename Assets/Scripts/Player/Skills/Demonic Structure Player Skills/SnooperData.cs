using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class SnooperData : DemonicStructurePlayerSkill {
    public override string name => "Snooper";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNOOPER;
    public override string description => "This Structure produces Eyes. The Player may place Eyes all over the world to obtain information and store them as Intel.";
    public SnooperData() {
        structureType = STRUCTURE_TYPE.SNOOPER;
    }
    //protected override string InvalidMessage(LocationGridTile tile) {
    //    if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.BEHOLDER)) {
    //        return "You can only have 1 Eye per region.";
    //    }
    //    return base.InvalidMessage(tile);
    //}
}
