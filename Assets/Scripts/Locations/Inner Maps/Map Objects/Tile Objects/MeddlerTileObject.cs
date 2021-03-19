using System.Collections.Generic;
using UnityEngine;

public class MeddlerTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,6f);

    public MeddlerTileObject() {
        Initialize(TILE_OBJECT_TYPE.MEDDLER_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public MeddlerTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Meddler"; }
}