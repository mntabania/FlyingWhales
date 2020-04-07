using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

public class GenericTileObject : TileObject {
    private bool hasBeenInitialized { get; set; }
    public override LocationGridTile gridTileLocation => _owner;
    private readonly LocationGridTile _owner;
    public GenericTileObject(LocationGridTile locationGridTile) {
        _owner = locationGridTile;
    }
    public GenericTileObject(SaveDataTileObject data) {
        Initialize(data, false);
    }

    #region Override
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        Messenger.Broadcast(Signals.TILE_OBJECT_REMOVED, this as TileObject, removedBy, removedFrom, destroyTileSlots);
        if (hasCreatedSlots && destroyTileSlots) {
            DestroyTileSlots();
        }
    }
    public override void OnPlacePOI() {
        SetPOIState(POI_STATE.ACTIVE);
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) { } //overridden this to reduce unnecessary processing 
    public override void OnDestroyPOI() {
        DisableGameObject();
        OnRemoveTileObject(null, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override void RemoveTileObject(Character removedBy) {
        LocationGridTile previousTile = this.gridTileLocation;
        DisableGameObject();
        OnRemoveTileObject(removedBy, previousTile);
        SetPOIState(POI_STATE.INACTIVE);
    }
    public override bool IsValidCombatTarget() {
        return false;
    }
    public override void OnTileObjectGainedTrait(Trait trait) {
        if (trait is Status status) {
            if(status.IsTangible()) {
                //if status is wet, and this tile is not part of a settlement, then do not create a map visual, since
                //characters do not react to wet tiles outside their settlement.
                bool willCreateVisual = !(status is Wet && gridTileLocation.IsPartOfSettlement() == false);
                if (willCreateVisual) {
                    GetOrCreateMapVisual();
                    SubscribeListeners();    
                }
                
            }
        }
        base.OnTileObjectGainedTrait(trait);
    }
    public override void OnTileObjectLostTrait(Trait trait) {
        base.OnTileObjectLostTrait(trait);
        if (TryDestroyMapVisual()) {
            UnsubscribeListeners();
        }
    }
    public override string ToString() {
        return $"Generic Obj at tile {gridTileLocation}";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        this.currentHP += amount;
        this.currentHP = Mathf.Clamp(this.currentHP, 0, maxHP);
        if (amount < 0) {
            Character responsibleCharacter = null;
            if (source != null && source is Character) {
                responsibleCharacter = source as Character;
            }
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }
        if (currentHP <= 0) {
            //floor has been destroyed
            gridTileLocation.RevertToPreviousGroundVisual();
            gridTileLocation.SetPreviousGroundVisual(null); //so that tile will never revert to old floor
            structureLocation.OnTileDestroyed(gridTileLocation);
            //reset floor hp
            currentHP = maxHP;
        } else if (amount < 0 && currentHP < maxHP) {
            //floor has been damaged
            structureLocation.OnTileDamaged(gridTileLocation);
        } else if (currentHP == maxHP) {
            //floor has been fully repaired
            structureLocation.OnTileRepaired(gridTileLocation);
        }
    }
    public override bool CanBeDamaged() {
        //only damage tiles that are part of non open space structures i.e structures with walls.
        return structureLocation.structureType.IsOpenSpace() == false
               && structureLocation.structureType.IsSettlementStructure();
    }
    public override bool CanBeSelected() {
        return false;
    }
    #endregion

    public BaseMapObjectVisual GetOrCreateMapVisual() {
        if (ReferenceEquals(mapVisual, null)) {
            InitializeMapObject(this);
            PlaceMapObjectAt(gridTileLocation);
            OnPlaceTileObjectAtTile(gridTileLocation);
        }
        return mapVisual;
    }
    public bool TryDestroyMapVisual() {
        if (traitContainer.HasTangibleTrait() == false) {
            if (ReferenceEquals(mapVisual, null) == false) {
                DestroyMapVisualGameObject();
            }
            return true;
        }
        return false;
    }

    public void ManualInitialize(LocationGridTile tile) {
        if (hasBeenInitialized) {
            return;
        }
        hasBeenInitialized = true;
        Initialize(TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, false);
        SetGridTileLocation(tile);
        // OnPlacePOI();
        // DisableGameObject();
        // RemoveCommonAdvertisements();
    }
}