using System.Collections.Generic;
using UnityEngine;

public class PortalTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,3f);
    public PortalTileObject() {
        Initialize(TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT);
    }
    public PortalTileObject(SaveDataTileObject data) {
        Initialize(data);
    }
    
    public override void ConstructDefaultActions() {
        actions = new List<SPELL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Demonic Portal"; }
}