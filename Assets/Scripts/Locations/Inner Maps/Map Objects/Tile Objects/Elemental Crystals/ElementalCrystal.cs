using System.Collections.Generic;
using Inner_Maps;
using Traits;

public abstract class ElementalCrystal : TileObject {
    protected ELEMENTAL_TYPE elementalType;

    protected ElementalCrystal(ELEMENTAL_TYPE _elementalType) : base() {
        elementalType = _elementalType;
    }
    public ElementalCrystal(SaveDataTileObject data) : base(data)  {
        elementalType = ELEMENTAL_TYPE.Normal;
    }

    #region Overrides
    public override void OnDestroyPOI() {
        List<LocationGridTile> affectedTiles = new List<LocationGridTile>() {previousTile};
        affectedTiles.AddRange(previousTile.neighbourList);
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            tile.PerformActionOnTraitables(DealElementalDamage);
        }
        base.OnDestroyPOI();
    }
    #endregion

    private void DealElementalDamage(ITraitable traitable) {
        traitable.AdjustHP(-50, elementalType, true, this, showHPBar: true);
    }
}