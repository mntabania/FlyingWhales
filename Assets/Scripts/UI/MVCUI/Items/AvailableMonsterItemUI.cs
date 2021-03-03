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
    public SummonSettings summonSettings;
    public SUMMON_TYPE summonType;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public RuinarchText txtManaCost;
    public RuinarchText txtChargeCount;

    public bool isMinion;

    public Image imgIcon;

    public GameObject disabler;

    public void InitializeItem(CharacterClass p_class, SummonSettings p_settings, SUMMON_TYPE p_summonType, int p_manaCost, int p_chargeCount, int p_maxChargeCount) {
        summonType = p_summonType;
        summonSettings = p_settings;
        characterClass = p_class;
        currentCharges = p_chargeCount;
        maxCharges = p_maxChargeCount;
        txtName.text = p_class.className;
        txtHP.text = p_class.baseHP.ToString();
        txtAtk.text = p_class.baseAttackPower.ToString();
        txtAtkSpd.text = p_class.baseAttackSpeed.ToString();
        txtManaCost.text = p_manaCost.ToString();
        txtChargeCount.text = currentCharges.ToString() + "/" + maxCharges.ToString();
        imgIcon.sprite = p_settings.summonPortrait;
        if (currentCharges  <= 0) {
            DisableButton(); 
        } else {
            EnableButton();
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
        Debug.LogError(currentCharges);
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