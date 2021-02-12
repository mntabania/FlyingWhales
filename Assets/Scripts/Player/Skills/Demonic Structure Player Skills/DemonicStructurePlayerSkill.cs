using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class DemonicStructurePlayerSkill : SkillData {
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE;
    public override string description => name;

    public STRUCTURE_TYPE structureType { get; protected set; }

    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }
    
    public override void ActivateAbility(Area targetArea) {
        string question;
        if (targetArea.IsNextToOrPartOfVillage()) {
            question = $"<color=\"red\">Warning: You are building too close to a village!</color>";
            question += "\nAre you sure you want to build " + name + "?";
        } else {
            question = "Are you sure you want to build " + name + "?";
        }
        UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", question, () => targetArea.structureComponent.StartBuild(type, targetArea), showCover: true, pauseAndResume: true, layer: 50);
        
        // base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        if (base.CanPerformAbilityTowards(targetArea)) {
            return targetArea.structureComponent.CanBuildDemonicStructureHere(structureType, targetArea);
        }
        return false;
    }
    public void BuildDemonicStructureAt(Area targetHex) {
        targetHex.gridTileComponent.StartCorruption(targetHex);
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(targetHex.settlementOnArea, targetHex.region.innerMap, targetHex, structureType, RESOURCE.NONE);
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    }
    
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        Color color = Color.green;
        color.a = 0.3f;
        TileHighlighter.Instance.PositionHighlight(tile.area, color);
    }
    public override bool InvalidHighlight(LocationGridTile tile, ref string invalidText) {
        Color color = Color.red;
        color.a = 0.3f;
        TileHighlighter.Instance.PositionHighlight(tile.area, color);
        invalidText = InvalidMessage(tile);
        return true;
    }
    protected virtual string InvalidMessage(LocationGridTile tile) { return string.Empty; }
}