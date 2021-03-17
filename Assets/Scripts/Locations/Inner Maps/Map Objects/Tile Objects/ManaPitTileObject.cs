using System.Collections.Generic;
using UnityEngine;

public class ManaPitTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(2f,2f);

    public ManaPitTileObject() {
        Initialize(TILE_OBJECT_TYPE.MANA_PIT_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public ManaPitTileObject(SaveDataTileObject data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Mana Pit"; }
}