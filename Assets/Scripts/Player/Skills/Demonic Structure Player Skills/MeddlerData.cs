using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class MeddlerData : DemonicStructurePlayerSkill {
    
    public override string name => "Meddler";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MEDDLER;
    public override string description => "This Structure allows the player to trigger wars between factions. It can also be used to goad Villagers into leaving or joining a Faction of your choice.";
    public MeddlerData() {
        structureType = STRUCTURE_TYPE.MEDDLER;
    }
    protected override string InvalidMessage(LocationGridTile tile) {
        if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER)) {
            return "You can only have 1 Meddler per world.";
        }
        return base.InvalidMessage(tile);
    }
}
