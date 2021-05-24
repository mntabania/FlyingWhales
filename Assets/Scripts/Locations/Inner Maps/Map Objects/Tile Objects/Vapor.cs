using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
public class Vapor : MovingTileObject {

    private VaporMapObjectVisual _vaporMapVisualObject;
    public int size { get; private set; }
    public int stacks { get; private set; }
    public GameDate expiryDate { get; }
    public bool doExpireEffect { get; private set; }
    protected override int affectedRange => size;
    public int maxSize => 6;
    public override System.Type serializedData => typeof(SaveDataVapor);
    
    public Vapor() {
        Initialize(TILE_OBJECT_TYPE.VAPOR, false);
        traitContainer.RemoveTrait(this, "Flammable");
        SetDoExpireEffect(true);
        expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
    }
    public Vapor(SaveDataVapor data) : base(data) {
        //SaveDataVapor saveDataVapor = data as SaveDataVapor;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        SetStacks(data.stacks);
        hasExpired = data.hasExpired;
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
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount < 0) { //&& source != null
            //CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source is Character character) {
                responsibleCharacter = character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
        }
        if (elementalDamageType == ELEMENTAL_TYPE.Fire) {
            _vaporMapVisualObject.Expire();
        } else if (elementalDamageType == ELEMENTAL_TYPE.Ice) {
            //Frosty Fog
            LocationGridTile targetTile = gridTileLocation;
            SetDoExpireEffect(false);
            _vaporMapVisualObject.Expire();
            FrostyFog frostyFog = new FrostyFog();
            frostyFog.SetGridTileLocation(targetTile);
            frostyFog.OnPlacePOI();
            frostyFog.SetStacks(stacks);
        } else if (elementalDamageType == ELEMENTAL_TYPE.Poison) {
            LocationGridTile targetTile = gridTileLocation;
            SetDoExpireEffect(false);
            _vaporMapVisualObject.Expire();
            InnerMapManager.Instance.SpawnPoisonCloud(targetTile, stacks, GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
        }
        if (!hasExpired && currentHP == 0) {
            _vaporMapVisualObject.Expire();
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
#endif
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
        if (_vaporMapVisualObject != null) {
            _vaporMapVisualObject.SetSize(size);    
        }
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
                gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(gridTileLocation.tileObjectComponent.genericTileObject, "Wet");
                Wet wet = gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet");
                wet?.SetIsPlayerSource(isPlayerSource);
            } else {
                List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
                gridTileLocation.PopulateTilesInRadius(tiles, radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].tileObjectComponent.genericTileObject.traitContainer.AddTrait(tiles[i].tileObjectComponent.genericTileObject, "Wet");
                    Wet wet = tiles[i].tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<Wet>("Wet");
                    wet?.SetIsPlayerSource(isPlayerSource);
                }
                RuinarchListPool<LocationGridTile>.Release(tiles);
            }
        }
    }
#endregion
}
#region Save Data
public class SaveDataVapor : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public int stacks;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Vapor vapor = tileObject as Vapor;
        Assert.IsNotNull(vapor);
        expiryDate = vapor.expiryDate;
        stacks = vapor.stacks;
    }
}
#endregion