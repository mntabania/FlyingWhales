using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public sealed class PoisonCloudTileObject : MovingTileObject {

    private PoisonCloudMapObjectVisual _poisonCloudVisual;
    
    public PoisonCloudTileObject() {
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Dangerous");
    }
    
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _poisonCloudVisual = mapVisual as PoisonCloudMapObjectVisual;
    }
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_poisonCloudVisual != null) {
            if (_poisonCloudVisual.isSpawned) {
                tile = _poisonCloudVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    public override void Neutralize() {
        _poisonCloudVisual.Expire();
    }
}
