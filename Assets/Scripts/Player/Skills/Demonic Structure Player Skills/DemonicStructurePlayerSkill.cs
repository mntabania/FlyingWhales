using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class DemonicStructurePlayerSkill : SpellData {
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEMONIC_STRUCTURE;
    public override string description => name;

    public STRUCTURE_TYPE structureType { get; protected set; }

    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.HEX };
    }
    
    public override void ActivateAbility(HexTile targetHex) {
        string question;
        if (targetHex.IsNextToOrPartOfVillage()) {
            question = $"<color=\"red\">Warning: You are building too close to a village!</color>";
            question += "\nAre you sure you want to build " + name + "?";
        } else {
            question = "Are you sure you want to build " + name + "?";
        }
        UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", question, () => targetHex.StartBuild(type), showCover: true, pauseAndResume: true);
        
        // base.ActivateAbility(targetHex);
    }
    public override bool CanPerformAbilityTowards(HexTile targetHex) {
        if (base.CanPerformAbilityTowards(targetHex)) {
            return targetHex.CanBuildDemonicStructureHere(structureType);
        }
        return false;
    }
    public void BuildDemonicStructureAt(HexTile targetHex) {
        targetHex.StartCorruption();
        LandmarkManager.Instance.PlaceBuiltStructureForSettlement(targetHex.settlementOnTile, targetHex.region.innerMap, targetHex, structureType, RESOURCE.NONE);
        targetHex.landmarkOnTile?.OnFinishedBuilding();
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    }
    
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        Color color = Color.green;
        color.a = 0.3f;
        TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner, color);
    }
    public override bool InvalidHighlight(LocationGridTile tile, ref string invalidText) {
        if (tile.collectionOwner.isPartOfParentRegionMap) {
            Color color = Color.red;
            color.a = 0.3f;
            TileHighlighter.Instance.PositionHighlight(tile.collectionOwner.partOfHextile.hexTileOwner, color);
            invalidText = InvalidMessage(tile);
            return true;    
        }
        return false;
    }
    protected virtual string InvalidMessage(LocationGridTile tile) { return string.Empty; }
}