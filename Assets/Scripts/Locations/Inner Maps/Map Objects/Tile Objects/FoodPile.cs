using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    #endregion
}
