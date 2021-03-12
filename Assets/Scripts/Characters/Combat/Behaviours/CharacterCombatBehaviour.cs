using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class CharacterCombatBehaviour {
    public CHARACTER_COMBAT_BEHAVIOUR behaviourType { get; private set; }
    public string name { get; private set; }

    public CharacterCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_type) {
        behaviourType = p_type;
        name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(behaviourType.ToString());
    }

    #region Virtuals
    public virtual void SetAsCombatBehaviourOf(Character p_character) { }
    public virtual void UnsetAsCombatBehaviourOf(Character p_character) { }
    public virtual void OnCharacterJoinedPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) { }
    public virtual void OnCharacterLeftPartyQuest(Character p_character, PARTY_QUEST_TYPE p_questType) { }
    #endregion
}
