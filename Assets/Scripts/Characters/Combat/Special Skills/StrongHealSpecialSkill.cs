using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongHealSpecialSkill : CombatSpecialSkill {
    public StrongHealSpecialSkill() : base(COMBAT_SPECIAL_SKILL.Strong_Heal, COMBAT_SPECIAL_SKILL_TARGET.Single, 20) {

    }

    #region Overrides
    public override bool TryActivateSkill(Character p_character) {
        Character validTarget = GetValidTargetFor(p_character);
        if(validTarget != null) {
            validTarget.AdjustHP(500, ELEMENTAL_TYPE.Normal);
            GameManager.Instance.CreateParticleEffectAt(validTarget, PARTICLE_EFFECT.Heal, false);
            p_character.logComponent.PrintLogIfActive("HEAL SPECIAL SKILL OF " + p_character.name + " ACTIVATED FOR: " + validTarget.name);
            return true;
        }
        return base.TryActivateSkill(p_character);
    }
    protected override Character GetValidTargetFor(Character p_character) {
        if (p_character.hasMarker) {
            Character lowestHPFriendlyCharacter = null;
            int lowestHP = 0;
            for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++) {
                Character visionCharacter = p_character.marker.inVisionCharacters[i];
                if(!visionCharacter.isDead && !visionCharacter.IsHealthFull()) {
                    if (p_character.faction != null 
                        && visionCharacter.faction != null 
                        && p_character.faction.IsFriendlyWith(visionCharacter.faction)
                        && !p_character.combatComponent.IsHostileInRange(visionCharacter)
                        && !p_character.combatComponent.IsAvoidInRange(visionCharacter)) {
                        if (lowestHPFriendlyCharacter == null || visionCharacter.currentHP < lowestHP) {
                            lowestHPFriendlyCharacter = visionCharacter;
                            lowestHP = visionCharacter.currentHP;
                        }
                    }
                }
            }
            return lowestHPFriendlyCharacter;
        }
        return base.GetValidTargetFor(p_character);
    }
    #endregion
}
