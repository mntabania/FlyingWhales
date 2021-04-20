﻿using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class PoisonCloud : MovingTileObject {

    private PoisonCloudMapObjectVisual _poisonCloudVisual;
    public GameDate expiryDate { get; private set; }
    public int size { get; private set; }
    public int stacks { get; private set; }
    public int maxSize => 6;
    public bool doExpireEffect { get; private set; }
    public override string neutralizer => "Poison Expert";
    protected override int affectedRange => size;
    public override System.Type serializedData => typeof(SaveDataPoisonCloud);
    
    public PoisonCloud() {
        Initialize(TILE_OBJECT_TYPE.POISON_CLOUD, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2)));
        
        SetDoExpireEffect(true);
    }
    public PoisonCloud(SaveDataPoisonCloud data) {
        //SaveDataPoisonCloud saveDataPoisonCloud = data as SaveDataPoisonCloud;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        hasExpired = data.hasExpired;
    }

    public void SetStacks(int stacks) {
        this.stacks = stacks;
        if (stacks >= 10) {
            traitContainer.AddTrait(this, "Dangerous");
        } else {
            traitContainer.RemoveTrait(this, "Dangerous");
        }
        UpdateSizeBasedOnPoisonedStacks();
    }
    public void Explode() {
        _poisonCloudVisual.Explode();
    }
    public void SetDoExpireEffect(bool state) {
        doExpireEffect = state;
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
    public void OnExpire() {
        if (doExpireEffect) {
            ExpireEffect();
        }
    }
    public override bool CanBeAffectedByElementalStatus(string traitName) {
        return false;
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        traitContainer.RemoveTrait(this, "Flammable");
    }
    #endregion

    #region Duration
    public void SetExpiryDate(GameDate expiryDate) {
        this.expiryDate = expiryDate;
    }
    #endregion

    #region Size
    private void SetSize(int size) {
        this.size = size;
        if (_poisonCloudVisual != null) {
            _poisonCloudVisual.SetSize(size);    
        }
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
    
    #region Expire Effect
    private void ExpireEffect() {
        if (gridTileLocation != null) {
            int radius = 0;
            if (UtilityScripts.Utilities.IsEven(size)) {
                //-3 because (-1 + -2), wherein -1 is the lower odd number, and -2 is the radius
                //So if size is 4, that means that 4-3 is 1, the radius for the 4x4 is 1 meaning we will get the neighbours of the tile.
                radius = size - 3;
            } else {
                //-2 because we will not need to get the lower odd number since this is already an odd number, just get the radius, hence, -2
                radius = size - 2;
            }
            //If the radius is less than or equal to zero this means we will only get the gridTileLocation itself
            if (radius <= 0) {
                gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTileLocation.tileObjectComponent.genericTileObject, "Poisoned", bypassElementalChance: true);
                Poisoned poisoned = gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
                if (poisoned != null) {
                    poisoned.SetIsPlayerSource(isPlayerSource);
                }
            } else {
                List<LocationGridTile> tiles = gridTileLocation.GetTilesInRadius(radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(tiles[i].tileObjectComponent.genericTileObject, "Poisoned", bypassElementalChance: true);
                    Poisoned poisoned = tiles[i].tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
                    if (poisoned != null) {
                        poisoned.SetIsPlayerSource(isPlayerSource);
                    }
                }
            }
        }
    }
    #endregion
}
#region Save Data
public class SaveDataPoisonCloud : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public int stacks;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        PoisonCloud poisonCloud = tileObject as PoisonCloud;
        Assert.IsNotNull(poisonCloud);
        expiryDate = poisonCloud.expiryDate;
        stacks = poisonCloud.stacks;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        PoisonCloud poisonCloud = tileObject as PoisonCloud;
        Assert.IsNotNull(poisonCloud);
        poisonCloud.SetStacks(stacks);
        return tileObject;
    }
}
#endregion