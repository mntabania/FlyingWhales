using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StonePile : ResourcePile {

    public StonePile() : base(RESOURCE.STONE) {
        Initialize(TILE_OBJECT_TYPE.STONE_PILE, false);
        SetResourceInPile(100);
    }
    public StonePile(SaveDataTileObject data) : base(data, RESOURCE.STONE) { }
    
    public override string ToString() {
        return $"Stone Pile {id.ToString()}";
    }
    public virtual bool CanBeReplaced() {
        return true;
    }
}