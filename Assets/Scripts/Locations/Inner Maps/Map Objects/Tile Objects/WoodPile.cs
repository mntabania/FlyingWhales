using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WoodPile : ResourcePile {
    public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Wood;
    public WoodPile() : base(RESOURCE.WOOD) {
        Initialize(TILE_OBJECT_TYPE.WOOD_PILE, false);
        SetResourceInPile(100);
    }
    public WoodPile(SaveDataTileObject data) : base(data, RESOURCE.WOOD) { }
    public override string ToString() {
        return $"Wood Pile {id.ToString()}";
    }
    public virtual bool CanBeReplaced() {
        return true;
    }
}