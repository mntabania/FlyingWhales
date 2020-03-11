using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class LocustSwarm : PlayerSpell {

    public LocustSwarm() : base(SPELL_TYPE.LOCUST_SWARM) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        tier = 1;
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        LocustSwarmTileObject tornadoTileObject = new LocustSwarmTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 2);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 2);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return tile.structure != null;
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return tile.structure != null;
    }
    #endregion
}

public class LocustSwarmData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LOCUST_SWARM;
    public override string name { get { return "Locust Swarm"; } }
    public override string description { get { return "These starving insects will consume all edible objects in its path."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 2;

    public LocustSwarmData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        LocustSwarmTileObject tornadoTileObject = new LocustSwarmTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(2, tile);
    }
}
