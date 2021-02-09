using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Logs;
using UnityEngine;
using Traits;

public class IgniteData : PlayerAction {
    private SkillData m_skillData;
    private PlayerSkillData m_playerSkillData;
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.IGNITE;
    public override string name => "Ignite";
    public override string description => "This Action can be used to apply Burning to an object.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.PLAYER_ACTION;
    public IgniteData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        m_skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.IGNITE);
        m_playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.IGNITE);
        LocationGridTile targetTile = targetPOI.gridTileLocation;

        if (targetTile != null) {
            BurningSource bs = null;
            targetTile.PerformActionOnTraitables((traitable) => IgniteEffect(traitable, ref bs));
        }
        
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", name, "activated", null, LOG_TAG.Player);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.gridTileLocation.genericTileObject.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")) {
            return false;
        }
        if (!tileObject.traitContainer.HasTrait("Flammable")) {
            return false;
        }
        if (tileObject.traitContainer.HasTrait("Burnt")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    #endregion

    private void IgniteEffect(ITraitable traitable, ref BurningSource bs) {
        if (traitable.gridTileLocation == null) { return; }
        Trait trait = null;
        if (traitable.traitContainer.AddTrait(traitable, "Burning", out trait, bypassElementalChance: true, 
            overrideDuration: (int)m_playerSkillData.skillUpgradeData.GetDurationBonusPerLevel(m_skillData.currentLevel))) {
            TraitManager.Instance.ProcessBurningTrait(traitable, trait, ref bs);
        }
    }
}