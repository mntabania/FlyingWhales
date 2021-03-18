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
    public RuinarchText txtStatus;
    public Image imgPortrait;

    public RuinarchText txtUnlockPrice;
    public GameObject lockCover;
    public GameObject emptyCover;

    public int unlockPrice;

    public bool isReadyForDeploy;
    public bool isDeployed;
    public bool isMinion;

    private MonsterAndDemonUnderlingCharges _monsterOrMinion;
    public MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;
    public Character deployedCharacter;

    public MonsterAndDemonUnderlingCharges monsterUnderlingCharges;

    public GameObject manaIconAndPrice;
    private void OnEnable() {
        btnMonster.onClick.AddListener(OnClicked);
        btnUnlock.onClick.AddListener(OnUnlocked);
	}

	private void OnDisable() {
        btnMonster.onClick.RemoveListener(OnClicked);
        btnUnlock.onClick.RemoveListener(OnUnlocked);
    }

	public void InitializeItem(MonsterAndDemonUnderlingCharges p_underling, bool p_isDeployed = false, bool p_hideRemoveButton = false) {
        _monsterOrMinion = p_underling;
        txtUnlockPrice.text = unlockPrice.ToString();
        txtName.text = p_underling.characterClass.className;
        txtHP.text = p_underling.characterClass.baseHP.ToString();
        txtAtk.text = p_underling.characterClass.baseAttackPower.ToString();
        txtAtkSpd.text = p_underling.characterClass.baseAttackSpeed.ToString();
        if (p_underling.isDemon) {
            isMinion = true;
            imgPortrait.sprite = CharacterManager.Instance.GetMinionSettings(p_underling.minionType).minionPortrait;
        } else {
            isMinion = false;
            imgPortrait.sprite = CharacterManager.Instance.GetSummonSettings(p_underling.monsterType).summonPortrait;
        }
        
        if (!p_isDeployed) {
            isReadyForDeploy = true;
            isDeployed = false;
            txtStatus.text = "Ready";
        } else {
            txtStatus.text = "Deployed";
            isReadyForDeploy = false;
            isDeployed = true;
            
        }
        if (p_hideRemoveButton) {
            HideRemoveButton();
        } else {
            ShowRemoveButton();
        }
        lockCover.SetActive(false);
        emptyCover.SetActive(false);
    }

    public void MakeSlotEmpty() {
        isDeployed = false;
        isReadyForDeploy = false;
        emptyCover.SetActive(true);
    }

    public void ResetButton() {
        isDeployed = false;
        isReadyForDeploy = false;
        emptyCover.SetActive(true);
    }

    public void MakeSlotLocked() {
        isDeployed = false;
        isReadyForDeploy = false;
        emptyCover.SetActive(false);
        lockCover.SetActive(true);
    }

    public void EnableButton() {
        btnMonster.interactable = true;
        lockCover.SetActive(false);
    }

    void OnClicked() {
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

    public void UndeployCharacter() {
        deployedCharacter = null;
    }

    public void HideManaCost() {
        manaIconAndPrice.gameObject.SetActive(false);
    }

    public void ShowManaCost() {
        manaIconAndPrice.gameObject.SetActive(true);
    }

    public void Deploy(Character p_createdCharacter = null, bool p_dontHideremoveButton = false) {
        deployedCharacter = p_createdCharacter;
        txtStatus.text = "Deployed";
        isDeployed = true;
        isReadyForDeploy = false;
        if (p_dontHideremoveButton) {
            ShowRemoveButton();
        } else {
            HideRemoveButton(); 
        }
        
    }

    public void HideRemoveButton() {
        btnMonster.gameObject.SetActive(false);
    }

    public void ShowRemoveButton() {
        btnMonster.gameObject.SetActive(true);
    }
}