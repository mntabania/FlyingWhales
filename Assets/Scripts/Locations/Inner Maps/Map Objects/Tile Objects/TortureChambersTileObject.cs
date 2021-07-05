using System.Collections.Generic;
using UnityEngine;

public class TortureChambersTileObject : TileObject {
    
    public override Vector2 selectableSize => new Vector2(3f,3f);
    public override Vector3 worldPosition => mapVisual.visionTrigger.transform.position;
    public override Vector3 attackRangePosition => GetAttackRangePosForDemonicStructureTileObject();
    public TortureChambersTileObject() {
        Initialize(TILE_OBJECT_TYPE.TORTURE_CHAMBERS_TILE_OBJECT);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.AddTrait(this, "Immovable");
    }
    public TortureChambersTileObject(SaveDataTileObject data) : base(data) {
        
    }
    public override bool CanBeSelected() {
        return true;
    }
    public override bool IsCurrentlySelected() { return false; }
    public override void LeftSelectAction() {
        // UIManager.Instance.ShowStructureInfo(gridTileLocation.structure);
    }
    // public override bool IsCurrentlySelected() {
    //     if (gridTileLocation != null) {
    //         return UIManager.Instance.structureInfoUI.isShowing &&
    //                UIManager.Instance.structureInfoUI.activeStructure == gridTileLocation.structure;    
    //     }
    //     return base.IsCurrentlySelected();
    // }
    public override void RightSelectAction() { }
    public override void MiddleSelectAction() { }
    public override void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //portal has no actions by default
    }
    protected override string GenerateName() { return "Prison"; }
}