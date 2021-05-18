using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "High HP. Taunts hostiles.";

    public TankCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Tank) {

    }

    #region Override
    public override bool DetermineCombatBehaviour(Character p_character, CombatState p_combatState) {
        if (p_character.combatComponent.combatBehaviourParent.canDoTankBehaviour) {
            if (p_character.partyComponent.isMemberThatJoinedQuest && p_character.hasMarker) {
                Character highestHPHostile = null;
                int highestHP = 0;
                for (int i = 0; i < p_character.marker.inVisionCharacters.Count; i++) {
                    Character visionCharacter = p_character.marker.inVisionCharacters[i];
                    if (!visionCharacter.isDead) {
                        if (visionCharacter.combatComponent.IsCurrentlyAttackingPartyMateOf(p_character)) {
                            if (highestHPHostile == null || visionCharacter.currentHP > highestHP) {
                                highestHPHostile = visionCharacter;
                                highestHP = visionCharacter.currentHP;
                            }
                        }
                    }
                }
                if (highestHPHostile != null) {
#if DEBUG_LOG
                    p_character.logComponent.PrintLogIfActive("TANK BEHAVIOUR FOR: " + p_character.name + ", TARGET: " + highestHPHostile.name);
#endif
                    if (p_character.combatComponent.IsHostileInRange(highestHPHostile)) {
                        p_combatState.SetForcedTarget(highestHPHostile);
                        p_character.combatComponent.combatBehaviourParent.SetCanDoTankBehaviour(false);
                        return true;
                    } else if (p_character.combatComponent.Fight(highestHPHostile, CombatManager.Tanking)) {
                        p_combatState.SetForcedTarget(highestHPHostile);
                        p_character.combatComponent.combatBehaviourParent.SetCanDoTankBehaviour(false);
                        return true;
                    }
                }
            }
        }
        return base.DetermineCombatBehaviour(p_character, p_combatState);
    }
#endregion
}
