using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MeddlerData : DemonicStructurePlayerSkill {
    
    public override string name => "Meddler";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MEDDLER;
    public override string description => "This Structure allows the player to start various Schemes.";
    public MeddlerData() {
        structureType = STRUCTURE_TYPE.MEDDLER;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER)) {
            return "You can only have 1 Meddler structure at a time.";
        }
        return base.InvalidMessage(tile);
    }
}
