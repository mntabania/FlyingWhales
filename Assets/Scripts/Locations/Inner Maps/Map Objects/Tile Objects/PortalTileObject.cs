﻿using System.Collections.Generic;
using UnityEngine;

public class PortalTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,3f);
    public override Vector3 worldPosition {
        get {
            Vector3 pos = mapVisual.transform.position;
            pos.x -= 0.5f;
            return pos;
        }
    }
    public PortalTileObject() {
        Initialize(TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public PortalTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Demonic Portal"; }
}