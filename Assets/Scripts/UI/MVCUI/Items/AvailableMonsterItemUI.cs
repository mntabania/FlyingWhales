using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableMonsterItemUI : MonoBehaviour
{
    public Action<AvailableMonsterItemUI> onClicked;
    public Button myButton;

    public int maxCharges;
    public int currentCharges;
    public CharacterClass characterClass;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public RuinarchText txtManaCost;
    public RuinarchText txtChargeCount;

    public bool isMinion;

    public Image imgIcon;

    public GameObject disabler;

    public void InitializeItem(CharacterClass p_class, Sprite p_portrait, int p_manaCost, int p_chargeCount, bool p_isDisabled = false) {
        characterClass = p_class;
        currentCharges = maxCharges = p_chargeCount;
        txtName.text = p_class.className;
        txtHP.text = p_class.baseHP.ToString();
        txtAtk.text = p_class.baseAttackPower.ToString();
        txtAtkSpd.text = p_class.baseAttackSpeed.ToString();
        txtManaCost.text = p_manaCost.ToString();
        txtChargeCount.text = currentCharges.ToString() + "/" + maxCharges.ToString();
        imgIcon.sprite = p_portrait;
        if (!p_isDisabled) {
            EnableButton();
        } else {
            DisableButton();
        }
    }

    public void DeductOneCharge(bool isDisabled = false) {
        currentCharges--;
        txtChargeCount.text = currentCharges.ToString() + "/" + maxCharges.ToString();
        if (currentCharges <= 0) {
            DisableButton();
        } else {
            EnableButton();
        }
        if (isDisabled) {
            DisableButton();
        }
    }

    public void AddOneCharge(bool isDisabled = false) {
        currentCharges++;
        txtChargeCount.text = currentCharges.ToString() + "/" + maxCharges.ToString();
        EnableButton();
        if (isDisabled) {
            DisableButton();
        }
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
	}

	private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
    }

	public void EnableButton() {
        myButton.interactable = true;
        disabler.SetActive(false);
    }

    public void DisableButton() {
        myButton.interactable = false;
        disabler.SetActive(true);
    }

    void Click() {
        onClicked?.Invoke(this);
    }
}