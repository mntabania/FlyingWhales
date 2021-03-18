using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderCombatBehaviour : CharacterCombatBehaviour {
    public override string description => "Gains bonus stats when used for defense.";
    public DefenderCombatBehaviour() : base(CHARACTER_COMBAT_BEHAVIOUR.Defender) {

    }

    #region Overrides
    public override void OnCharacterJoinedPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) {
        if(p_questType == PARTY_QUEST_TYPE.Demon_Defend) {
            p_character.combatComponent.AdjustMaxHPModifier(500);
            p_character.combatComponent.AdjustAttackModifier(50);
        }
    }
    public override void OnCharacterLeftPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) {
        if (p_questType == PARTY_QUEST_TYPE.Demon_Defend) {
            p_character.combatComponent.AdjustMaxHPModifier(-500);
            p_character.combatComponent.AdjustAttackModifier(-50);
        }
    }
    #endregion
}
