using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class DeployedMonsterItemUI : MonoBehaviour {

    public Action<DeployedMonsterItemUI> onClicked;
    public Action<DeployedMonsterItemUI> onUnlocked;

    public Button btnMonster;
    public Button btnUnlock;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public Image imgPortrait;

    public RuinarchText txtUnlockPrice;
    public GameObject lockCover;
    public GameObject emptyCover;

    public int unlockPrice;

    public bool isDeployed;
    public CharacterClass characterClass;

	private void OnEnable() {
        btnMonster.onClick.AddListener(OnClicked);
        btnUnlock.onClick.AddListener(OnUnlocked);
	}

	private void OnDisable() {
        btnMonster.onClick.RemoveListener(OnClicked);
        btnUnlock.onClick.RemoveListener(OnUnlocked);
    }

	public void InitializeItem(CharacterClass p_class, Sprite p_sprite) {
        characterClass = p_class;
        txtUnlockPrice.text = unlockPrice.ToString();
        txtName.text = p_class.className;
        txtHP.text = p_class.baseHP.ToString();
        txtAtk.text = p_class.baseAttackPower.ToString();
        txtAtkSpd.text = p_class.baseAttackSpeed.ToString();

        imgPortrait.sprite = p_sprite;

        emptyCover.SetActive(false);
    }

    public void MakeSlotEmpty() {
        emptyCover.SetActive(true);
    }

    public void EnableButton() {
        btnMonster.interactable = true;
        lockCover.SetActive(false);
    }

    public void DisableButton() {
        btnMonster.interactable = false;
        lockCover.SetActive(true);
    }

    void OnClicked() {
        MakeSlotEmpty();
        isDeployed = false;
        onClicked?.Invoke(this);
    }

    void OnUnlocked() {
        if (PlayerManager.Instance.player.mana >= unlockPrice) {
            PlayerManager.Instance.player.AdjustMana(-unlockPrice);
            EnableButton();
            MakeSlotEmpty();
        }
        onUnlocked?.Invoke(this);
    }
}