﻿using System.Collections.Generic;
using UnityEngine;

public class BeholderTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(3f,4f);
    public override Vector3 worldPosition => mapVisual.visionTrigger.transform.position;
    
    public BeholderTileObject() {
        Initialize(TILE_OBJECT_TYPE.BEHOLDER_TILE_OBJECT);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.AddTrait(this, "Immovable");
    }
    public BeholderTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return true;
    }
    public override void LeftSelectAction() {
        UIManager.Instance.ShowStructureInfo(gridTileLocation.structure);
    }
    public override void RightSelectAction() { }
    public override void MiddleSelectAction() { }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Beholder"; }
}