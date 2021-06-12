using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;

public abstract class ElementalCrystal : TileObject {
    protected ELEMENTAL_TYPE elementalType;

    protected ElementalCrystal(ELEMENTAL_TYPE _elementalType) : base() {
        elementalType = _elementalType;
    }
    public ElementalCrystal(SaveDataTileObject data, ELEMENTAL_TYPE _elementalType) : base(data)  {
        elementalType = _elementalType;
    }

    #region Overrides
    public override void OnDestroyPOI() {
        List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
        if (previousTile != null) {
            affectedTiles.Add(previousTile);
        }
        affectedTiles.AddRange(previousTile.neighbourList);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(DealElementalDamage);
        }
        RuinarchListPool<LocationGridTile>.Release(affectedTiles);
        base.OnDestroyPOI();
    }
    #endregion

    private void DealElementalDamage(ITraitable traitable) {
        traitable.AdjustHP(-50, elementalType, true, this, showHPBar: true);
    }
}