using System.Collections.Generic;
using UnityEngine;

public class BeholderTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(3f,4f);

    public BeholderTileObject() {
        Initialize(TILE_OBJECT_TYPE.BEHOLDER_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public BeholderTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Beholder"; }
}