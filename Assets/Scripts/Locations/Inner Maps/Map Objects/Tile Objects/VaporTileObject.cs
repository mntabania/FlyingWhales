using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class VaporTileObject : MovingTileObject {

    private VaporMapObjectVisual _vaporMapVisualObject;
    public int size { get; private set; }
    public int stacks { get; private set; }
    public int maxSize { get; private set; }
    public bool doExpireEffect { get; private set; }
    protected override int affectedRange => size;
    
    public VaporTileObject() {
        Initialize(TILE_OBJECT_TYPE.VAPOR, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        traitContainer.RemoveTrait(this, "Flammable");
        maxSize = 6;
        SetDoExpireEffect(true);
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _vaporMapVisualObject = mapVisual as VaporMapObjectVisual;
        Assert.IsNotNull(_vaporMapVisualObject, $"Map Object Visual of {this} is null!");
    }
    public void SetDoExpireEffect(bool state) {
        doExpireEffect = state;
    }
    public void SetStacks(int stacks) {
        this.stacks = stacks;
        UpdateSizeBasedOnWetStacks();
    }
    public override void Neutralize() {
        _vaporMapVisualObject.Expire();
    }
    public void OnExpire() {
        //Messenger.Broadcast<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
        if (doExpireEffect) {
            ExpireEffect();
        }
    }
    public override string ToString() {
        return "Vapor";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { //&& source != null
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        if (elementalDamageType == ELEMENTAL_TYPE.Fire) {
            _vaporMapVisualObject.Expire();
        } else if (elementalDamageType == ELEMENTAL_TYPE.Ice) {
            //Frosty Fog
            LocationGridTile targetTile = gridTileLocation;
            SetDoExpireEffect(false);
            _vaporMapVisualObject.Expire();
            FrostyFogTileObject frostyFog = new FrostyFogTileObject();
            frostyFog.SetGridTileLocation(targetTile);
            frostyFog.OnPlacePOI();
            frostyFog.SetStacks(stacks);
        } else if (elementalDamageType == ELEMENTAL_TYPE.Poison) {
            LocationGridTile targetTile = gridTileLocation;
            SetDoExpireEffect(false);
            _vaporMapVisualObject.Expire();
            PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
            poisonCloudTileObject.SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6)));
            poisonCloudTileObject.SetGridTileLocation(targetTile);
            poisonCloudTileObject.OnPlacePOI();
            poisonCloudTileObject.SetStacks(stacks);
        }
        if (!hasExpired && currentHP == 0) {
            _vaporMapVisualObject.Expire();
        }
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
    }

    #region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_vaporMapVisualObject != null) {
            if (_vaporMapVisualObject.isSpawned) {
                tile = _vaporMapVisualObject.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
    #endregion

    #region Size
    private void SetSize(int size) {
        this.size = size;
        _vaporMapVisualObject.SetSize(size);
    }
    private void UpdateSizeBasedOnWetStacks() {
        if (stacks >= 1 && stacks <= 2) {
            SetSize(1);
        } else if (stacks >= 3 && stacks <= 4) {
            SetSize(2);
        } else if (stacks >= 5 && stacks <= 9) {
            SetSize(3);
        } else if (stacks >= 10 && stacks <= 16) {
            SetSize(4);
        } else if (stacks >= 17 && stacks <= 25) {
            SetSize(5);
        } else if (stacks >= 26) {
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
                gridTileLocation.genericTileObject.traitContainer.AddTrait(gridTileLocation.genericTileObject, "Wet");
            } else {
                List<LocationGridTile> tiles = gridTileLocation.GetTilesInRadius(radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].genericTileObject.traitContainer.AddTrait(tiles[i].genericTileObject, "Wet");
                }
            }
        }
    }
    #endregion
}
