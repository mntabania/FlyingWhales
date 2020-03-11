using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataInterventionAbility {
    public SPELL_TYPE abilityType;
    public int level;

    public void Save(PlayerJobActionSlot slot) {
        if(slot.ability != null) {
            abilityType = slot.ability.spellType;
        } else {
            abilityType = SPELL_TYPE.NONE;
        }
        level = slot.level;
    }
    public PlayerJobActionSlot Load() {
        PlayerJobActionSlot slot = new PlayerJobActionSlot();
        slot.SetLevel(level);
        if (abilityType != SPELL_TYPE.NONE) {
            PlayerSpell ability = PlayerManager.Instance.CreateNewInterventionAbility(abilityType);
            slot.SetAbility(ability);
        }
        return slot;
    }
}
