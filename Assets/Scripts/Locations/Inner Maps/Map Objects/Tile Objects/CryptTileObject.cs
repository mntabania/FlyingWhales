using System.Collections.Generic;
using UnityEngine;

public class CryptTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(4f,3f);

    public CryptTileObject() {
        Initialize(TILE_OBJECT_TYPE.CRYPT_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public CryptTileObject(SaveDataTileObject data) {
        
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
    protected override string GenerateName() { return "Crypt"; }
}