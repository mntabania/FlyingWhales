﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; private set; }
    public int resourceInPile => storedResources[providedResource];
    public ResourcePile(RESOURCE providedResource) {
        AddAdvertisedAction(INTERACTION_TYPE.TAKE_RESOURCE);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
        //AddAdvertisedAction(INTERACTION_TYPE.DROP);
        AddAdvertisedAction(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.BOOBY_TRAP);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        this.providedResource = providedResource;
    }
    public ResourcePile(SaveDataTileObject data, RESOURCE providedResource) : base(data) {
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        SetResource(providedResource, amount);
        if(resourceInPile <= 0 && gridTileLocation != null && isBeingCarriedBy == null) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        AdjustResource(providedResource, adjustment);
        Messenger.Broadcast(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, this);
        if (resourceInPile <= 0) {
            if(gridTileLocation != null && isBeingCarriedBy == null) {
                gridTileLocation.structure.RemovePOI(this);
            } else if (isBeingCarriedBy != null) {
                //If amount in pile was reduced to zero and is still being carried, remove from being carried and destroy it
                isBeingCarriedBy.UncarryPOI(this, addToLocation: false);
            }
        }
    }
    public virtual bool HasResource() {
        return resourceInPile > 0;
    }
    protected override void ConstructMaxResources() {
        maxResourceValues = new Dictionary<RESOURCE, int>();
        RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            //only allow resource type of what this resource pile provides.
            maxResourceValues.Add(resourceType, resourceType == providedResource ? 1000 : 0);
        }
    }
    #endregion

    #region Overrides
    protected override string GetNameplateName() {
        return $"{name} (x{resourceInPile.ToString()})";
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.DESTROY, this as IPointOfInterest);
    }
    protected override void OnSetObjectAsUnbuilt() {
        base.OnSetObjectAsUnbuilt();
        AddAdvertisedAction(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE);
    }
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tResource in Pile: {resourceInPile.ToString()}";
        return data;
    }
    public override void SetCharacterOwner(Character characterOwner) { } //do not set character owner of resource pile. Reference: https://trello.com/c/TRzgjik6/1352-resources-like-food-pile-and-wood-pile-should-not-have-owners-at-any-time
    #endregion
}