using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfHealSpecialSkill : CombatSpecialSkill {
    public SelfHealSpecialSkill() : base(COMBAT_SPECIAL_SKILL.Self_Heal, COMBAT_SPECIAL_SKILL_TARGET.Single, 20) {

    }

    #region Overrides
    public override bool TryActivateSkill(Character p_character) {
        Character validTarget = GetValidTargetFor(p_character);
        if(validTarget != null) {
            validTarget.AdjustHP(100, ELEMENTAL_TYPE.Normal);
            GameManager.Instance.CreateParticleEffectAt(validTarget, PARTICLE_EFFECT.Heal, false);
#if DEBUG_LOG
            p_character.logComponent.PrintLogIfActive("SELF HEAL SPECIAL SKILL OF " + p_character.name + " ACTIVATED FOR: " + validTarget.name);
#endif
            p_character.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(3, p_character);
            return true;
        }
        return base.TryActivateSkill(p_character);
    }
    protected override Character GetValidTargetFor(Character p_character) {
        return p_character;
    }
#endregion
}
