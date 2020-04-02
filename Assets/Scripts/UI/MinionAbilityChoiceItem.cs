using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinionAbilityChoiceItem : MonoBehaviour {

    public Minion minion { get; private set; }
    public string abilityIdentifier { get; private set; }

    public CharacterPortrait portrait;
    public TextMeshProUGUI minionText;
    public GridLayoutGroup abilityGrid;
    public Toggle toggle;
    public GameObject abilityItemPrefab;

   public void SetMinion(Minion minion, string abilityIdentifier) {
        this.minion = minion;
        this.abilityIdentifier = abilityIdentifier;
        if (minion != null) {
            portrait.GeneratePortrait(minion.character);

            string text = minion.character.name;
            text += $"\n{minion.character.raceClassName}";
            minionText.text = text;

            UpdateAbilityItems();
        }
    }

    private void UpdateAbilityItems() {
        UtilityScripts.Utilities.DestroyChildren(abilityGrid.transform);
        if(abilityIdentifier == "combat") {
            GameObject go = GameObject.Instantiate(abilityItemPrefab, abilityGrid.transform);
            AbilityItem item = go.GetComponent<AbilityItem>();
            item.SetAbility(minion.combatAbility);
        } 
        //else if (abilityIdentifier == "intervention") {
        //    for (int i = 0; i < minion.unlockedInterventionSlots; i++) {
        //        GameObject go = GameObject.Instantiate(abilityItemPrefab, abilityGrid.transform);
        //        AbilityItem item = go.GetComponent<AbilityItem>();
        //        item.SetAbility(minion.interventionAbilities[i]);
        //    }
        //}
    }

    public void OnClickThis(bool selected) {
        if (selected) {
            PlayerUI.Instance.newMinionAbilityUI.OnSelectChoice(minion);
        }
    }
}
