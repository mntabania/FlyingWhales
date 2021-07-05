using System.Collections.Generic;
using UnityEngine;

public class DemonicStructureBlockerTileObject : TileObject {
    

    public DemonicStructureBlockerTileObject() {
        Initialize(TILE_OBJECT_TYPE.DEMONIC_STRUCTURE_BLOCKER_TILE_OBJECT);
        RemoveAdvertisedAction(INTERACTION_TYPE.STEAL_ANYTHING);
        traitContainer.AddTrait(this, "Immovable");
    }
    public DemonicStructureBlockerTileObject(SaveDataTileObject data) : base(data) {
        
    }
    public override bool CanBeSelected() {
        return false;
    }
    public override bool IsUnpassable() {
        return true;
    }
    // public override void LeftSelectAction() {
    //     UIManager.Instance.ShowStructureInfo(gridTileLocation.structure);
    // }
    // public override bool IsCurrentlySelected() {
    //     if (gridTileLocation != null) {
    //         return UIManager.Instance.structureInfoUI.isShowing &&
    //                UIManager.Instance.structureInfoUI.activeStructure == gridTileLocation.structure;    
    //     }
    //     return base.IsCurrentlySelected();
    // }
    // public override void RightSelectAction() { }
    // public override void MiddleSelectAction() { }
    // public override void ConstructDefaultActions() {
    //     actions = new List<PLAYER_SKILL_TYPE>();
    //     //portal has no actions by default
    // }
    protected override string GenerateName() { return "Demonic Structure Blocker"; }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, 
        CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0, bool isPlayerSource = false) {
        showHPBar = false; //never show hp bar for this tile object
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource);
    }
}