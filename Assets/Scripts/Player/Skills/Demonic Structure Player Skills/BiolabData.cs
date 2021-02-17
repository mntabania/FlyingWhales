using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class BiolabData : DemonicStructurePlayerSkill {
    public override string name => "Biolab";
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BIOLAB;
    public override string description => "This Structure allows the player to customize a plague using Plague Points.";
    public BiolabData() {
        structureType = STRUCTURE_TYPE.BIOLAB;
    }
    
    protected override string InvalidMessage(LocationGridTile tile) {
        if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB)) {
            return "You can only have 1 Biolab built at a time.";
        }
        return base.InvalidMessage(tile);
    }
}
