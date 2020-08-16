using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataCombatAbility {
    public COMBAT_ABILITY type;
    public int lvl;

    public void Save(CombatAbility ability) {
        type = ability.type;
        lvl = ability.lvl;
    }
    public void Load(Minion minion) {
        CombatAbility ability = PlayerManager.Instance.CreateNewCombatAbility(type);
        //minion.SetCombatAbility(ability);
        ability.SetLevel(lvl);
    }
}
