using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;

public class PoisonCloud : PlayerSpell {
    

    public PoisonCloud() : base(SPELL_TYPE.POISON_CLOUD) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        PoisonCloudTileObject tornadoTileObject = new PoisonCloudTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
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

public class PoisonCloudData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.POISON_CLOUD;
    public override string name => "Poison Cloud";
    public override string description => "This noxious cloud may apply poison to objects in its path. Explodes when it gets in contact with Fire, dealing Normal damage.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public PoisonCloudData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
        poisonCloudTileObject.SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6)));
        poisonCloudTileObject.SetGridTileLocation(targetTile);
        poisonCloudTileObject.OnPlacePOI();
        poisonCloudTileObject.SetStacks(5);
        IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(2, tile);
    }
}