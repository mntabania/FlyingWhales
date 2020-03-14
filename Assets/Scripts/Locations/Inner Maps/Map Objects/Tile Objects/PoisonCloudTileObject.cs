using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class PoisonCloudTileObject : MovingTileObject {

    private PoisonCloudMapObjectVisual _poisonCloudVisual;
    public int durationInTicks { get; private set; }
    public int size { get; private set; }
    
    public PoisonCloudTileObject() {
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Dangerous");
        SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(2));
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
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_STACKED, OnTraitStacked);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_REMOVED, OnTraitRemoved);
        Messenger.AddListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_UNSTACKED, OnTraitUnstacked);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_ADDED, OnTraitAdded);
        Messenger.RemoveListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_STACKED, OnTraitStacked);
        Messenger.RemoveListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_REMOVED, OnTraitRemoved);
        Messenger.RemoveListener<TileObject, Trait>(Signals.TILE_OBJECT_TRAIT_UNSTACKED, OnTraitUnstacked);
    }

    #region Listeners
    private void OnTraitAdded(TileObject tileObject, Trait trait) {
        if (tileObject == this && trait is Poisoned) {
            UpdateSizeBasedOnPoisonedStacks();
        }
    }
    private void OnTraitRemoved(TileObject tileObject, Trait trait) {
        if (tileObject == this && trait is Poisoned) {
            UpdateSizeBasedOnPoisonedStacks();
        }
    }
    private void OnTraitStacked(TileObject tileObject, Trait trait) {
        if (tileObject == this && trait is Poisoned) {
            UpdateSizeBasedOnPoisonedStacks();
        }
    }
    private void OnTraitUnstacked(TileObject tileObject, Trait trait) {
        if (tileObject == this && trait is Poisoned) {
            UpdateSizeBasedOnPoisonedStacks();
        }
    }
    #endregion

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
        int poisonedStacks = 1;
        if (traitContainer.stacks.ContainsKey("Poisoned")) {
            poisonedStacks = traitContainer.stacks["Poisoned"];    
        }
        Assert.IsTrue(poisonedStacks > 0, $"Poisoned stacks of {this} is {poisonedStacks}.");
        
        if (UtilityScripts.Utilities.IsInRange(poisonedStacks, 1, 3)) {
            SetSize(1);
        } else if (UtilityScripts.Utilities.IsInRange(poisonedStacks, 3, 5)) {
            SetSize(2);
        } else if (UtilityScripts.Utilities.IsInRange(poisonedStacks, 5, 10)) {
            SetSize(3);
        } else if (UtilityScripts.Utilities.IsInRange(poisonedStacks, 10, 17)) {
            SetSize(4);
        } else if (UtilityScripts.Utilities.IsInRange(poisonedStacks, 17, 26)) {
            SetSize(5);
        } else {
            SetSize(6);
        }
    }
    #endregion
}
