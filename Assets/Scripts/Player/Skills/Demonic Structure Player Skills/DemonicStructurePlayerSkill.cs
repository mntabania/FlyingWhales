using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class DemonicStructurePlayerSkill : SkillData {
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE;
    public override string description => name;

    public STRUCTURE_TYPE structureType { get; protected set; }

    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        string question;
        if (targetTile.area.IsNextToOrPartOfVillage()) {
            question = $"<color=\"red\">Warning: You are building too close to a village!</color>";
            question += "\nAre you sure you want to build " + name + "?";
        } else {
            question = "Are you sure you want to build " + name + "?";
        }
        UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", question, () => BuildDemonicStructure(targetTile), showCover: true, pauseAndResume: true, layer: 50);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        if (base.CanPerformAbilityTowards(targetTile)) {
            return targetTile.area.CanBuildDemonicStructureHere(structureType); 
        }
        return false;
    }
    // public void BuildDemonicStructureAt(Area targetArea) {
    //     //targetArea.StartCorruption();
    //     LandmarkManager.Instance.PlaceBuiltStructureForSettlement(targetArea.settlementOnArea, targetArea.region.innerMap, targetArea, structureType, RESOURCE.NONE);
    //     //targetHex.landmarkOnTile?.OnFinishedBuilding();
    //     Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    // }
    private void BuildDemonicStructure(LocationGridTile p_tile) {
        p_tile.InstantPlaceDemonicStructure(new StructureSetting(structureType, RESOURCE.NONE));
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    }
    
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(tile.area, GameUtilities.GetValidTileHighlightColor());
    }
    public override bool InvalidHighlight(LocationGridTile tile, ref string invalidText) {
        TileHighlighter.Instance.PositionHighlight(tile.area, GameUtilities.GetInvalidTileHighlightColor());
        invalidText = InvalidMessage(tile);
        return true;
    }
    protected virtual string InvalidMessage(LocationGridTile tile) { return string.Empty; }
}