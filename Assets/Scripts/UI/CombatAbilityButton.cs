using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatAbilityButton : MonoBehaviour {

    public CombatAbility ability { get; private set; }

    public Button button;
    public Image buttonImage;
    public Image activeAbilityImage;
    public Image coverImage;

    public void SetCombatAbility(CombatAbility ability) {
        this.ability = ability;
        if(ability != null) {
            buttonImage.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
        }
        UpdateInteractableState();
    }
    public void UpdateInteractableState() {
        button.interactable = ability != null && ability != PlayerManager.Instance.player.currentActiveCombatAbility && !ability.IsInCooldown();
        if (button.IsInteractable()) {
            coverImage.gameObject.SetActive(false);
            //activeAbilityImage.gameObject.SetActive(false);
        }else {
            coverImage.gameObject.SetActive(true);
            //activeAbilityImage.gameObject.SetActive(true);
        }
    }

    public void OnClickButton() {
        PlayerManager.Instance.player.SetCurrentActiveCombatAbility(ability);
    }

    public void ShowHoverText() {
        UIManager.Instance.ShowSmallInfo(ability.name);
    }
    public void HideHoverText() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnCombatAbilityUpdateButton(CombatAbility ability) {
        if(this.ability == ability) {
            UpdateInteractableState();
        }
    }
}
