using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealData : PlayerAction {
    private SkillData m_skillData;
    private PlayerSkillData m_playerSkillData;
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.HEAL;
    public override string name => "Heal";
    public override string description => "This Action fully replenishes a character's HP.";
    public HealData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        m_skillData = PlayerSkillManager.Instance.GetPlayerSkillData(PLAYER_SKILL_TYPE.HEAL);
        m_playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.HEAL);
        if (targetPOI is Character targetCharacter) {
            int processedHeal = (int)(targetCharacter.maxHP * m_playerSkillData.skillUpgradeData.GetAdditionalHpPercentagePerLevelBaseOnLevel(m_skillData.currentLevel));
            targetCharacter.AdjustHP(processedHeal, ELEMENTAL_TYPE.Normal, showHPBar: true,
                piercingPower: m_playerSkillData.skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(m_skillData.currentLevel));
        }
        base.ActivateAbility(targetPOI);
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if(targetCharacter.currentHP >= targetCharacter.maxHP) {
            return false;
        }
        return base.CanPerformAbilityTowards(targetCharacter);
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (targetCharacter.isDead) {
            reasons += $"{targetCharacter.name} is already dead,";
        }
        if (targetCharacter.currentHP >= targetCharacter.maxHP) {
            reasons += $"{targetCharacter.name} is at full HP,";
        }
        return reasons;
    }
    #endregion
}
