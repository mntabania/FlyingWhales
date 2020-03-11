using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionCharacterItem : CharacterNameplateItem {

    public void ShowCombatAbilityTooltip() {
        string header = character.minion.combatAbility.name;
        string message = character.minion.combatAbility.description;
        UIManager.Instance.ShowSmallInfo(message, header);
    }

    public override void Reset() {
        base.Reset();
    }
}
