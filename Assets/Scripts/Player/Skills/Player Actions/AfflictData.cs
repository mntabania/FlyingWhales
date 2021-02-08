using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

public class AfflictData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.AFFLICT;
    public override string name => "Afflict";
    public override string description => $"Afflict a Villager with a negative Trait.";

    public AfflictData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Character character) {
            UIManager.Instance.characterInfoUI.ShowAfflictUI();
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return targetCharacter.isDead == false;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is Character character) {
            if (!character.isNormalCharacter || character.isConsideredRatman) {
                return false;
            }
        }
        return base.IsValid(target);
    }
    protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> p_contextMenuItems) {
        if (type == PLAYER_SKILL_TYPE.AFFLICT && PlayerManager.Instance.player.currentlySelectedPlayerActionTarget != null) {
            p_contextMenuItems.Clear();
            List<PLAYER_SKILL_TYPE> afflictionTypes = PlayerManager.Instance.player.playerSkillComponent.afflictions;
            for (int i = 0; i < afflictionTypes.Count; i++) {
                PLAYER_SKILL_TYPE spellType = afflictionTypes[i];
                PlayerAction spellData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType) as PlayerAction;
                if (spellData != null && spellData.IsValid(PlayerManager.Instance.player.currentlySelectedPlayerActionTarget)) {
                    p_contextMenuItems.Add(spellData);
                }
            }
            return p_contextMenuItems;    
        }
        return null;
    }
    #endregion

    protected void AfflictPOIWith(string traitName, IPointOfInterest target, string logName) {
        target.traitContainer.AddTrait(target, traitName);
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "player_afflicted", null, LogUtilities.Player_Life_Changes_Tags);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, logName, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        LogPool.Release(log);
    }
}