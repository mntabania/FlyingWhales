using System.Collections.Generic;
using UnityEngine;

public class DefensePointTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(2f,2f);

    public DefensePointTileObject() {
        Initialize(TILE_OBJECT_TYPE.DEFENSE_POINT_TILE_OBJECT);
        traitContainer.AddTrait(this, "Immovable");
    }
    public DefensePointTileObject(SaveDataTileObject data) {
        
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
    protected override string GenerateName() { return "Defense Point"; }
}