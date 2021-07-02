using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
public class TauntSpecialSkill : CombatSpecialSkill {
    public TauntSpecialSkill() : base(COMBAT_SPECIAL_SKILL.Taunt, COMBAT_SPECIAL_SKILL_TARGET.Multiple, 5) {

    }

    #region Overrides
    public override bool TryActivateSkill(Character p_character) {
        bool hasActivated = false;
        List<Character> tauntedCharacters = RuinarchListPool<Character>.Claim();
        PopulateValidTargetsFor(p_character, tauntedCharacters);
        if(tauntedCharacters.Count > 0) {
            hasActivated = true;
            GameManager.Instance.CreateParticleEffectAt(p_character, PARTICLE_EFFECT.Taunt, false);
            for (int i = 0; i < tauntedCharacters.Count; i++) {
                Character taunted = tauntedCharacters[i];
                taunted.interruptComponent.TriggerInterrupt(INTERRUPT.Taunted, p_character);
            }
#if DEBUG_LOG
            p_character.logComponent.PrintLogIfActive("TAUNT SPECIAL SKILL OF " + p_character.name + " ACTIVATED!");
#endif
        }
        RuinarchListPool<Character>.Release(tauntedCharacters);
        return hasActivated;
    }
    protected override void PopulateValidTargetsFor(Character p_character, List<Character> p_validTargets) {
        if (p_character.hasMarker) {
            for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++) {
                Character visionCharacter = p_character.marker.inVisionCharacters[i];
                if (!visionCharacter.isDead && !visionCharacter.traitContainer.HasTrait("Taunted")) {
                    if (p_character.faction != null
                        && visionCharacter.faction != null
                        && p_character.faction.IsHostileWith(visionCharacter.faction)
                        && !p_character.combatComponent.IsAvoidInRange(visionCharacter)) {
                        if (visionCharacter.combatComponent.IsCurrentlyAttackingFriendlyWith(p_character)) {
                            p_validTargets.Add(visionCharacter);
                        } else if (p_character.faction != null && p_character.faction.isPlayerFaction) {
                            if (visionCharacter.combatComponent.IsCurrentlyAttackingDemonicStructure()) {
                                p_validTargets.Add(visionCharacter);
                            }
                        }
                    }
                }
            }
        }
    }
#endregion
}
