using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class SpawnBoulder : PlayerSpell {

    public SpawnBoulder() : base(SPELL_TYPE.SPAWN_BOULDER) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        targetTile.structure.AddPOI(wall, targetTile);
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        targetTile.HighlightTile();
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        targetTile.UnhighlightTile();
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return tile.structure != null && tile.objHere == null;
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return tile.structure != null && tile.objHere == null;
    }
    #endregion
}

public class SpawnBoulderData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.SPAWN_BOULDER;
    public override string name => "Spawn Boulder";
    public override string description => "This will produce a durable and impassable block of stone.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 2;

    public SpawnBoulderData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        targetTile.structure.AddPOI(wall, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null && targetTile.objHere == null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}

