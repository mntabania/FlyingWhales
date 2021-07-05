using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
public class FrostyFog : MovingTileObject {

    private FrostyFogMapObjectVisual _frostyFogMapVisual;
    public int size { get; private set; }
    public int stacks { get; private set; }
    public GameDate expiryDate { get; }
    public int maxSize => 6;
    
    public override System.Type serializedData => typeof(SaveDataFrostyFog);

    public FrostyFog() {
        Initialize(TILE_OBJECT_TYPE.FROSTY_FOG, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        traitContainer.RemoveTrait(this, "Flammable");
        expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
    }
    public FrostyFog(SaveDataFrostyFog data) : base(data) {
        //SaveDataFrostyFog saveDataFrostyFog = data as SaveDataFrostyFog;
        Assert.IsNotNull(data);
        expiryDate = data.expiryDate;
        SetStacks(data.stacks);
        hasExpired = data.hasExpired;
    }
    protected override void CreateMapObjectVisual() {
        base.CreateMapObjectVisual();
        _frostyFogMapVisual = mapVisual as FrostyFogMapObjectVisual;
        Assert.IsNotNull(_frostyFogMapVisual, $"Map Object Visual of {this} is null!");
    }
    public override void Neutralize() {
        _frostyFogMapVisual.Expire();
    }
    public void OnExpire() {
        Messenger.Broadcast<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, this, null, base.gridTileLocation);
    }
    public override string ToString() {
        return "Frosty Fog";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        LocationGridTile tileLocation = gridTileLocation;
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
            //Wet
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            tileLocation.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].AddTraitToAllPOIsOnTile("Wet");
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
            _frostyFogMapVisual.Expire();
        } else if (elementalDamageType == ELEMENTAL_TYPE.Electric) {
            //2 Ball Lightnings
            int numOfBallLightnings = 1 + (stacks / 4);
            if (numOfBallLightnings > 4) {
                numOfBallLightnings = 4;
            }
            for (int i = 0; i < numOfBallLightnings; i++) {
                BallLightning ballLightning = new BallLightning();
                ballLightning.SetGridTileLocation(tileLocation);
                ballLightning.OnPlacePOI();
            }
            _frostyFogMapVisual.Expire();
        } else if (currentHP == 0) {
            //object has been destroyed
            _frostyFogMapVisual.Expire();
        }
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}HP of {this} was adjusted by {amount}. New HP is {currentHP}.");
#endif
    }

#region Moving Tile Object
    protected override bool TryGetGridTileLocation(out LocationGridTile tile) {
        if (_frostyFogMapVisual != null) {
            if (_frostyFogMapVisual.isSpawned) {
                tile = _frostyFogMapVisual.gridTileLocation;
                return true;
            }
        }
        tile = null;
        return false;
    }
#endregion

#region Size and Stacks
    public void SetStacks(int stacks) {
        this.stacks = stacks;
        UpdateSizeBasedOnStacks();
    }
    private void SetSize(int size) {
        this.size = size;
        if (_frostyFogMapVisual != null) {
            _frostyFogMapVisual.SetSize(size);    
        }
    }
    private void UpdateSizeBasedOnStacks() {
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
}

#region Save Data
public class SaveDataFrostyFog : SaveDataMovingTileObject {
    public GameDate expiryDate;
    public int stacks;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        FrostyFog frostyFog = tileObject as FrostyFog;
        Assert.IsNotNull(frostyFog);
        expiryDate = frostyFog.expiryDate;
        stacks = frostyFog.stacks;
    }
}
#endregion
