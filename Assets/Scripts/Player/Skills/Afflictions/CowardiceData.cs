using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;

public class CowardiceData : AfflictData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.COWARDICE;
    public override string name => "Cowardice";
    public override string description => "This Affliction will turn a Villager into a Coward. Cowards will often run away from combat.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.AFFLICTION;
    public CowardiceData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.COWARDICE);
        SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.COWARDICE);
        AfflictPOIWith("Coward", targetPOI, name, playerSkillData.skillUpgradeData.GetDurationBonusPerLevel(skillData.currentLevel));
        OnExecutePlayerSkill();
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Coward")) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.traitContainer.HasTrait("Coward")) {
            reasons += $"{targetCharacter.name} already has this Flaw,";
        }
        return reasons;
    }
    #endregion
}