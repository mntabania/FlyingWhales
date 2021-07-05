using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Villager_Wants;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class Table : TileObject {
    public int food => resourceStorageComponent.storedResources[RESOURCE.FOOD];
    public CONCRETE_RESOURCES lastAddedFoodType { get; private set; }

    public override Type serializedData => typeof(SaveDataTable);

    public Table() {
        Initialize(TILE_OBJECT_TYPE.TABLE);
        AddAdvertisedAction(INTERACTION_TYPE.DRINK);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_RESOURCE);
        AddAdvertisedAction(INTERACTION_TYPE.SIT);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);

        SetFood(CONCRETE_RESOURCES.Animal_Meat, UnityEngine.Random.Range(20, 81)); //20
        // SetFood(CONCRETE_RESOURCES.Animal_Meat, 1000);
        traitContainer.AddTrait(this, "Edible");
    }

    public Table(SaveDataTileObject data) : base(data) {
        SaveDataTable saveDataTable = data as SaveDataTable;
        lastAddedFoodType = saveDataTable.lastAddedFoodType;
    }

    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        //if (IsSlotAvailable()) {
        //    //if (GetActiveUserCount() > 0) {
        //    UpdateUsedTableAsset();
        //    //} else {
        //    //    gridTileLocation.parentAreaMap.UpdateTileObjectVisual(this); //update visual based on state
        //    //}
        //}
    }
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);

        /*
         * RESOURCE[] resourceTypes = CollectionUtilities.GetEnumValues<RESOURCE>();
        for (int i = 0; i < resourceTypes.Length; i++) {
            RESOURCE resourceType = resourceTypes[i];
            if (resourceType != RESOURCE.NONE) {
                resourceStorageComponent.SetResourceCap(resourceType, resourceType == RESOURCE.FOOD ? 100 : 0);
            }
        }
        */
    }
    public override string ToString() {
        return $"Table {id.ToString()}";
    }
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                AddUser(action.actor);
                break;

        }
    }
    public override void OnDoneActionToObject(ActualGoapNode action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.SIT:
                RemoveUser(action.actor);
                break;

        }
    }
    protected override void OnPlaceTileObjectAtTile(LocationGridTile tile) {
        base.OnPlaceTileObjectAtTile(tile);
        if (mapVisual.usedSprite.name.Contains("bartop")) {
            mapVisual.InitializeGUS(new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), tile);
        } else { 
            mapVisual.InitializeGUS(Vector2.zero, new Vector2(0.5f, 0.5f), tile);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        mapVisual.DestroyExistingGUS();
    }
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data = $"{data}\n\tFood in Table: {food.ToString()}";
        data = $"{data}\n\tLast Added Food Type: {lastAddedFoodType.ToString()}";
        return data;
    }
    protected override void OnSetObjectAsUnbuilt() {
        base.OnSetObjectAsUnbuilt();
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
    protected override void OnSetObjectAsBuilt() {
        base.OnSetObjectAsBuilt();
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
    #endregion

    #region Users
    public bool CanAccommodateCharacter(Character p_character) {
        return HasUnoccupiedSlot();
    }
    #endregion

    #region Food
    public void AdjustFood(CONCRETE_RESOURCES p_foodType, int p_amount) {
        Assert.IsTrue(p_foodType.GetResourceCategory() == RESOURCE.FOOD, $"A non-food resource is being used to increase food resource of {nameWithID}");
        lastAddedFoodType = p_foodType;
        resourceStorageComponent.AdjustResource(p_foodType, p_amount);
        if (gridTileLocation != null && structureLocation is Dwelling) {
            Messenger.Broadcast(StructureSignals.FOOD_IN_DWELLING_CHANGED, this);   
        }
    }
    public void SetFood(CONCRETE_RESOURCES p_foodType, int p_amount) {
        Assert.IsTrue(p_foodType.GetResourceCategory() == RESOURCE.FOOD, $"A non-food resource is being used to increase food resource of {nameWithID}");
        lastAddedFoodType = p_foodType;
        resourceStorageComponent.SetResource(p_foodType, p_amount);
        if (gridTileLocation != null && structureLocation is Dwelling) {
            Messenger.Broadcast(StructureSignals.FOOD_IN_DWELLING_CHANGED, this);   
        }
    }
    #endregion

    #region Eating
    public void ApplyFoodEffectsToConsumer(Character p_consumer) {
        switch (lastAddedFoodType) {
            case CONCRETE_RESOURCES.Corn:
                p_consumer.traitContainer.AddTrait(p_consumer, "Corn Fed");
                break;
            case CONCRETE_RESOURCES.Potato:
                p_consumer.traitContainer.AddTrait(p_consumer, "Potato Fed");
                break;
            case CONCRETE_RESOURCES.Pineapple:
                p_consumer.traitContainer.AddTrait(p_consumer, "Pineapple Fed");
                break;
            case CONCRETE_RESOURCES.Iceberry:
                p_consumer.traitContainer.AddTrait(p_consumer, "Iceberry Fed");
                break;
            case CONCRETE_RESOURCES.Fish:
                p_consumer.traitContainer.AddTrait(p_consumer, "Fish Fed");
                break;
            case CONCRETE_RESOURCES.Animal_Meat:
                p_consumer.traitContainer.AddTrait(p_consumer, "Animal Fed");
                break;
        }
    }
    #endregion

    #region Reactions
    public override void VillagerReactionToTileObject(Character actor, ref string debugLog) {
        base.VillagerReactionToTileObject(actor, ref debugLog);
        TryCreateObtainFurnitureWantOnReactionJob<TableWant>(actor);
    }
    #endregion
}

public class SaveDataTable : SaveDataTileObject {
    public CONCRETE_RESOURCES lastAddedFoodType;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Table obj = tileObject as Table;
        lastAddedFoodType = obj.lastAddedFoodType;
    }
}