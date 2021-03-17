using System.Collections.Generic;
using UnityEngine;

public class BiolabTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,4f);

    public BiolabTileObject() {
        Initialize(TILE_OBJECT_TYPE.BIOLAB_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public BiolabTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Biolab"; }
}