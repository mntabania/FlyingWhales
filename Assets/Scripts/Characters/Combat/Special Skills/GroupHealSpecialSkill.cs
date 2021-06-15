using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupHealSpecialSkill : CombatSpecialSkill {
    public GroupHealSpecialSkill() : base(COMBAT_SPECIAL_SKILL.Group_Heal, COMBAT_SPECIAL_SKILL_TARGET.Multiple, 20) {

    }

    #region Overrides
    public override bool TryActivateSkill(Character p_character) {
        bool hasActivated = false;
        List<Character> healedCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
        PopulateValidTargetsFor(p_character, healedCharacters);
        if(healedCharacters.Count > 0) {
            hasActivated = true;
            for (int i = 0; i < healedCharacters.Count; i++) {
                Character target = healedCharacters[i];
                target.AdjustHP(100, ELEMENTAL_TYPE.Normal);
                GameManager.Instance.CreateParticleEffectAt(target, PARTICLE_EFFECT.Heal, false);
#if DEBUG_LOG
                p_character.logComponent.PrintLogIfActive("GROUP HEAL SPECIAL SKILL OF " + p_character.name + " ACTIVATED FOR: " + target.name);
#endif
            }
            p_character.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(3, p_character);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(healedCharacters);
        return hasActivated;
    }
    protected override void PopulateValidTargetsFor(Character p_character, List<Character> p_validTargets) {
        if (p_character.hasMarker) {
            if (!p_character.isDead && !p_character.IsHealthFull()) {
                p_validTargets.Add(p_character);
            }
            for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++) {
                Character visionCharacter = p_character.marker.inVisionCharacters[i];
                if(!visionCharacter.isDead && !visionCharacter.IsHealthFull()) {
                    if (p_character.faction != null 
                        && visionCharacter.faction != null 
                        && p_character.faction.IsFriendlyWith(visionCharacter.faction)
                        && !p_character.combatComponent.IsHostileInRange(visionCharacter)
                        && !p_character.combatComponent.IsAvoidInRange(visionCharacter)) {
                        p_validTargets.Add(visionCharacter);
                    }
                }
            }
        }
    }
#endregion
}
