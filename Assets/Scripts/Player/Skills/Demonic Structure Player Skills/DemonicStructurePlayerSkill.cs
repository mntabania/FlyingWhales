﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class DemonicStructurePlayerSkill : SkillData {
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE;
    public override string description => name;

    public STRUCTURE_TYPE structureType { get; protected set; }

    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.AREA };
    }
    
    public override void ActivateAbility(Area targetArea) {
        string question;
        if (targetArea.IsNextToOrPartOfVillage()) {
            question = $"<color=\"red\">Warning: You are building too close to a village!</color>";
            question += "\nAre you sure you want to build " + name + "?";
        } else {
            question = "Are you sure you want to build " + name + "?";
        }
        UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", question, () => targetArea.structureComponent.StartBuild(type), showCover: true, pauseAndResume: true, layer: 50);
        
        // base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(Area targetArea) {
        if (base.CanPerformAbilityTowards(targetArea)) {
            return targetArea.structureComponent.CanBuildDemonicStructureHere(structureType);
        }
        return false;
    }
    public void BuildDemonicStructureAt(Area targetArea) {
        PlayerManager.Instance.player.playerSettlement.AddAreaToSettlement(targetArea);
        targetArea.featureComponent.RemoveAllFeatures(targetArea);
        //targetArea.StartCorruption();
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(targetArea.settlementOnArea, targetArea.region.innerMap, targetArea, structureType, RESOURCE.NONE);
        //targetHex.landmarkOnTile?.OnFinishedBuilding();
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