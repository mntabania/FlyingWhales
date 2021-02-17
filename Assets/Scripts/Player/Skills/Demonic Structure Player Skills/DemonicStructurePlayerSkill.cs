using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

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
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        if (base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason)) {
            return targetTile.area.structureComponent.CanBuildDemonicStructureHere(structureType, out o_cannotPerformReason) && structureTemplate.HasEnoughSpaceIfPlacedOn(targetTile, out o_cannotPerformReason); 
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
        p_tile.PlaceSelfBuildingDemonicStructure(structureSetting, 5);
        Messenger.Broadcast(UISignals.UPDATE_BUILD_LIST);
    }
}