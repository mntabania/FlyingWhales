﻿using System.Collections.Generic;
using UnityEngine;

public class ImpHutTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,3f);

    public ImpHutTileObject() {
        Initialize(TILE_OBJECT_TYPE.IMP_HUT_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public ImpHutTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Imp Hut"; }
}