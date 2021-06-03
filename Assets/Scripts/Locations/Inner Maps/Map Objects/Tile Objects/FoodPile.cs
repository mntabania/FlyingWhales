using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public abstract class FoodPile : ResourcePile {
    
    protected FoodPile(TILE_OBJECT_TYPE tileObjectType) : base(RESOURCE.FOOD) {
        Initialize(tileObjectType, false);
        //traitContainer.RemoveTrait(this, "Flammable");
        traitContainer.AddTrait(this, "Edible");
        SetResourceInPile(20);
    }
    protected FoodPile(SaveDataTileObject saveDataTileObject) : base(saveDataTileObject, RESOURCE.FOOD) { }
    
    #region Overrides
    public override string ToString() {
        return $"Food Pile {id.ToString()}";
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (gridTileLocation != null && gridTileLocation.structure.structureType.IsFoodProducingStructure()) {
            AddAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
        } else {
            RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
        }
    }
    public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true) {
        base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
        RemoveAdvertisedAction(INTERACTION_TYPE.BUY_FOOD);
    }
    #endregion
}
