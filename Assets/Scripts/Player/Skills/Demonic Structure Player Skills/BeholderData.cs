using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class BeholderData : DemonicStructurePlayerSkill {
    public override string name => "Beholder";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BEHOLDER;
    public override string description => "This Structure produces Eyes. The Player may place Eyes all over the world to obtain information and store them as Intel.";
    public BeholderData() {
        structureType = STRUCTURE_TYPE.BEHOLDER;
    }
    //protected override string InvalidMessage(LocationGridTile tile) {
    //    if (tile.parentMap.region.HasStructure(STRUCTURE_TYPE.BEHOLDER)) {
    //        return "You can only have 1 Eye per region.";
    //    }
    //    return base.InvalidMessage(tile);
    //}
}
