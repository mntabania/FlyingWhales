using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Ignite : PlayerSpell {

    private List<LocationGridTile> highlightedTiles;

    public Ignite() : base(SPELL_TYPE.IGNITE) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        highlightedTiles = new List<LocationGridTile>();
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        List<LocationGridTile> tiles = GetTargetTiles(targetTile);
        if (tiles.Count > 0) {
            BurningSource bs = new BurningSource(targetTile.parentMap.region);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                Burning burning = new Burning();
                burning.SetSourceOfBurning(bs, tile.genericTileObject);
                tile.genericTileObject.traitContainer.AddTrait(tile.genericTileObject, burning);
            }
            Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        }
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return GetTargetTiles(tile).Count > 0;
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return GetTargetTiles(tile).Count > 0;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        highlightedTiles = GetTargetTiles(targetTile);
        InnerMapManager.Instance.HighlightTiles(highlightedTiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        InnerMapManager.Instance.UnhighlightTiles(highlightedTiles);
        highlightedTiles.Clear();
    }
    #endregion

    private List<LocationGridTile> GetTargetTiles(LocationGridTile origin) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        tiles.Add(origin);
         if (level == 2) {
            tiles.AddRange(origin.FourNeighbours());
        } else if (level >= 3) {
            tiles.AddRange(origin.neighbourList);
        }
        return tiles.Where(x => x.genericTileObject != null && !x.genericTileObject.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof") && x.genericTileObject.traitContainer.HasTrait("Flammable")).ToList();
    }
}

public class IgniteData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.IGNITE;
    public override string name { get { return "Ignite"; } }
    public override string description { get { return "Targets a spot. Target will ignite and start spreading fire."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }

    public IgniteData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        IncreaseThreatForEveryCharacterThatSeesPOI(targetPOI, 5);
        // LocationGridTile tile = targetPOI.gridTileLocation;
        BurningSource bs = new BurningSource(targetPOI.gridTileLocation.parentMap.region);
        Burning burning = new Burning();
        burning.SetSourceOfBurning(bs, targetPOI);
        targetPOI.traitContainer.AddTrait(targetPOI, burning, bypassElementalChance: true);
        Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning", "Wet", "Fireproof")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    // public override bool CanPerformAbilityTowards(SpecialToken item) {
    //     if (item.gridTileLocation == null || item.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning")) {
    //         return false;
    //     }
    //     return base.CanPerformAbilityTowards(item);
    // }
    #endregion
}