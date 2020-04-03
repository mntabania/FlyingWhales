using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class PoisonCloudTileObject : MovingTileObject {

    private PoisonCloudMapObjectVisual _poisonCloudVisual;
    public int durationInTicks { get; private set; }
    public int size { get; private set; }
    public int stacks { get; private set; }
    public int maxSize { get; private set; }
    public override string neutralizer => "Poison Expert";
    protected override int affectedRange => size;
    
    public PoisonCloudTileObject() {
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(2));
        maxSize = 6;
    }

    #region Overrides
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _poisonCloudVisual = mapVisual as PoisonCloudMapObjectVisual;
    }
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_poisonCloudVisual != null && hasExpired == false) {
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
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.AddTrait(this, "Dangerous");
        traitContainer.RemoveTrait(this, "Flammable");
    }
    #endregion
    
    public void SetStacks(int stacks) {
        this.stacks = stacks;
        UpdateSizeBasedOnPoisonedStacks();
    }

    #region Duration
    public void SetDurationInTicks(int duration) {
        durationInTicks = duration;
    }
    #endregion

    #region Size
    private void SetSize(int size) {
        this.size = size;
        _poisonCloudVisual.SetSize(size);
    }
    private void UpdateSizeBasedOnPoisonedStacks() {
        if (UtilityScripts.Utilities.IsInRange(stacks, 1, 3)) {
            SetSize(1);
        } else if (UtilityScripts.Utilities.IsInRange(stacks, 3, 5)) {
            SetSize(2);
        } else if (UtilityScripts.Utilities.IsInRange(stacks, 5, 10)) {
            SetSize(3);
        } else if (UtilityScripts.Utilities.IsInRange(stacks, 10, 17)) {
            SetSize(4);
        } else if (UtilityScripts.Utilities.IsInRange(stacks, 17, 26)) {
            SetSize(5);
        } else {
            SetSize(6);
        }
    }
    #endregion

    public void Explode() {
        _poisonCloudVisual.Explode();
    }
}
