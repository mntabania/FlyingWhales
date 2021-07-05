using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class SpawnBoulderData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.NONE;
    public override string name => "Spawn Boulder";
    public override string description => "This will produce a durable and impassable block of stone.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 2;

    public SpawnBoulderData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        targetTile.structure.AddPOI(wall, targetTile);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null && targetTile.tileObjectComponent.objHere == null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}

