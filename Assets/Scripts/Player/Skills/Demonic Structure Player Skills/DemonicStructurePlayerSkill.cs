using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using UnityEngine;

public class DemonicStructurePlayerSkill : SkillData {
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE;
    public override string description => name;

    public STRUCTURE_TYPE structureType { get; protected set; }
    private StructureSetting structureSetting => new StructureSetting(structureType, RESOURCE.NONE);
    private LocationStructureObject m_structureTemplate;

    #region getter
    public LocationStructureObject structureTemplate {
        get {
            if (m_structureTemplate == null) {
                m_structureTemplate = InnerMapManager.Instance.GetFirstStructurePrefabForStructure(structureSetting).GetComponent<LocationStructureObject>();
            }
            return m_structureTemplate;
        }
    }
    #endregion
    
    public DemonicStructurePlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(LocationGridTile targetTile) {
        PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(false);
        // string question;
        // if (targetTile.area.IsNextToOrPartOfVillage()) {
        //     question = $"<color=\"red\">Warning: You are building too close to a village!</color>";
        //     question += "\nAre you sure you want to build " + name + "?";
        // } else {
        //     question = "Are you sure you want to build " + name + "?";
        // }
        // UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", question, () => BuildDemonicStructure(targetTile), OnClickNoOnBuildStructureConfirmation, showCover: true, pauseAndResume: true, layer: 50);
        BuildDemonicStructure(targetTile);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason)) {
            return targetTile.area.structureComponent.CanBuildDemonicStructureHere(structureType, out o_cannotPerformReason) 
                && structureTemplate.HasEnoughSpaceIfPlacedOn(targetTile, out o_cannotPerformReason)
                && CanBuildDemonicStructureOn(structureTemplate, targetTile, out o_cannotPerformReason); 
        }
        return false;
    }
    public override void OnSetAsCurrentActiveSpell() {
        PlayerManager.Instance.ShowStructurePlacementVisual(structureType);
    }
    public override void OnNoLongerCurrentActiveSpell() {
        PlayerManager.Instance.HideStructurePlacementVisual();
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(GameUtilities.GetValidTileHighlightColor());
    }
    public override bool ShowInvalidHighlight(LocationGridTile tile, ref string invalidText) {
        PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(GameUtilities.GetInvalidTileHighlightColor());
        invalidText = InvalidMessage(tile);
        return true;
    }
    #endregion

    #region Virtuals
    protected virtual string InvalidMessage(LocationGridTile tile) { return string.Empty; }
    #endregion
    
    private void OnClickNoOnBuildStructureConfirmation() {
        PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(true);
    }
    private void BuildDemonicStructure(LocationGridTile p_tile) {
        p_tile.PlaceSelfBuildingDemonicStructure(structureSetting, 1);
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    }
    private bool CanBuildDemonicStructureOn(LocationStructureObject structureObj, LocationGridTile centerTile, out string o_cannotPlaceReason) {
        if (centerTile.corruptionComponent.isCorrupted || centerTile.corruptionComponent.IsTileAdjacentToACorruption()) {
            o_cannotPlaceReason = string.Empty;
            return true;
        }
        InnerTileMap map = centerTile.parentMap;
        for (int i = 0; i < structureObj.localOccupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = structureObj.localOccupiedCoordinates[i];

            Vector3Int gridTileLocation = centerTile.localPlace;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - structureObj.center.x;
            int yDiffFromCenter = currCoordinate.y - structureObj.center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;

            if (UtilityScripts.Utilities.IsInRange(gridTileLocation.x, 0, map.width)
                && UtilityScripts.Utilities.IsInRange(gridTileLocation.y, 0, map.height)) {
                LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
                if (tile.corruptionComponent.isCorrupted || tile.corruptionComponent.IsTileAdjacentToACorruption()) {
                    o_cannotPlaceReason = string.Empty;
                    return true;
                }
            } 
            //else {
            //    return false; //returned coordinates are out of the map
            //}
        }
        o_cannotPlaceReason = LocalizationManager.Instance.GetLocalizedValue("Locations", "Structures", "invalid_build_not_corrupted");
        return false;
    }
}