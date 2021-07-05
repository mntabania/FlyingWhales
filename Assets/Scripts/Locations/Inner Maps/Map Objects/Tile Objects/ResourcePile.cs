using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; private set; }
    public abstract CONCRETE_RESOURCES specificProvidedResource { get; }
    public int resourceInPile => resourceStorageComponent.storedResources[providedResource];
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
        AddAdvertisedAction(INTERACTION_TYPE.DROP_RESOURCE_TO_WORK_STRUCTURE);
        this.providedResource = providedResource;
    }
    public ResourcePile(SaveDataTileObject data, RESOURCE providedResource) : base(data) {
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        resourceStorageComponent.SetResource(specificProvidedResource, amount);
        if(resourceInPile <= 0 && gridTileLocation != null && isBeingCarriedBy == null) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        resourceStorageComponent.AdjustResource(specificProvidedResource, adjustment);
        Messenger.Broadcast(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, this);
        if (resourceInPile <= 0) {
            if(gridTileLocation != null && isBeingCarriedBy == null) {
                gridTileLocation.structure.RemovePOI(this);
            } else if (isBeingCarriedBy != null) {
                //If amount in pile was reduced to zero and is still being carried, remove from being carried and destroy it
                isBeingCarriedBy.UncarryPOI(this, addToLocation: false);
                eventDispatcher.ExecuteTileObjectDestroyed(this);
                Messenger.Broadcast(TileObjectSignals.DESTROY_TILE_OBJECT, this as TileObject);
            }
        }
    }
    public void OnPileCombinedToOtherPile() {
        eventDispatcher.ExecuteTileObjectDestroyed(this);
        Messenger.Broadcast(TileObjectSignals.DESTROY_TILE_OBJECT, this as TileObject);
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
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);

        /*
         * RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
         * for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            if (resourceType != RESOURCE.NONE) {
                //only allow resource type of what this resource pile provides.
                resourceStorageComponent.SetResourceCap(resourceType, resourceType == providedResource ? 1000 : 0);    
            }
        }
        */
    }
    public override void VillagerReactionToTileObject(Character actor, ref string debugLog) {
        base.VillagerReactionToTileObject(actor, ref debugLog);
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive
            && actor.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
            if (actor.partyComponent.currentParty.currentQuest is RaidPartyQuest raidParty
                && gridTileLocation != null && gridTileLocation.IsPartOfSettlement(raidParty.targetSettlement)) {
                if (GameUtilities.RollChance(35)) {
                    if (actor.jobComponent.TriggerStealRaidJob(this)) {
                        raidParty.SetIsSuccessful(true);
                    }
                }
            }
        }
    }
    protected override void OnSetGridTileLocation() {
        base.OnSetGridTileLocation();
#if DEBUG_LOG
        Debug.Log($"Grid tile location of {nameWithID} was set to {gridTileLocation?.ToString()}");
#endif
    }
    #endregion
    
}