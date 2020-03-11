using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

public class GenericTileObject : TileObject {

    public bool hasBeenInitialized { get; private set; }

    public GenericTileObject() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public GenericTileObject(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
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
        // if (ReferenceEquals(mapVisual, null)) {
        //     InitializeMapObject(this);
        // }
        // PlaceMapObjectAt(gridTileLocation);
        // OnPlaceTileObjectAtTile(gridTileLocation);
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
        base.OnTileObjectGainedTrait(trait);
        if (trait is Status status) {
            if(status.IsTangible()) {
                // EnableGameObject();
                //create map object visual
                if (ReferenceEquals(mapVisual, null)) {
                    InitializeMapObject(this);
                }
                PlaceMapObjectAt(gridTileLocation);
                OnPlaceTileObjectAtTile(gridTileLocation);

                SubscribeListeners();
            }
        }
    }
    public override void OnTileObjectLostTrait(Trait trait) {
        base.OnTileObjectLostTrait(trait);
        if (HasTangibleTrait() == false) {
            // DisableGameObject();
            if (ReferenceEquals(mapVisual, null) == false) {
                DestroyMapVisualGameObject();    
            }
            UnsubscribeListeners();
        }
    }
    public override string ToString() {
        return $"Generic Obj at tile {gridTileLocation}";
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (currentHP == 0 && amount < 0) {
            return; //hp is already at minimum, do not allow any more negative adjustments
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        this.currentHP += amount;
        this.currentHP = Mathf.Clamp(this.currentHP, 0, maxHP);
        if (amount <= 0) {
            Character responsibleCharacter = null;
            if (source != null && source is Character) {
                responsibleCharacter = source as Character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter);
        }
        if (currentHP <= 0) {
            //floor has been destroyed
            gridTileLocation.RevertToPreviousGroundVisual();
            gridTileLocation.SetPreviousGroundVisual(null); //so that tile will never revert to old floor
            structureLocation.OnTileDestroyed(gridTileLocation);
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
    #endregion

    private bool HasTangibleTrait() {
        for (int i = 0; i < traitContainer.statuses.Count; i++) {
            Status currTrait = traitContainer.statuses[i];
            if (currTrait.IsTangible()) {
                return true;
            }
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