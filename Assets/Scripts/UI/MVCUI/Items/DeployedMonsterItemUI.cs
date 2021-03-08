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
    public CharacterClass characterClass;
    public SummonSettings summonSettings;
    public SUMMON_TYPE summonType;
    public PLAYER_SKILL_TYPE playerSkillType;
    public Character deployedCharacter;
    private void OnEnable() {
        btnMonster.onClick.AddListener(OnClicked);
        btnUnlock.onClick.AddListener(OnUnlocked);
	}

	private void OnDisable() {
        btnMonster.onClick.RemoveListener(OnClicked);
        btnUnlock.onClick.RemoveListener(OnUnlocked);
    }

	public void InitializeItem(CharacterClass p_class, SummonSettings p_settings, SUMMON_TYPE p_summonType, bool p_isDeployed = false) {
        summonType = p_summonType;
        summonSettings = p_settings;
        characterClass = p_class;
        txtUnlockPrice.text = unlockPrice.ToString();
        txtName.text = p_class.className;
        txtHP.text = p_class.baseHP.ToString();
        txtAtk.text = p_class.baseAttackPower.ToString();
        txtAtkSpd.text = p_class.baseAttackSpeed.ToString();

        imgPortrait.sprite = p_settings.summonPortrait;
        if (!p_isDeployed) {
            isReadyForDeploy = true;
            isDeployed = false;
            txtStatus.text = "Ready";
        } else {
            txtStatus.text = "Deployed";
            isReadyForDeploy = false;
            isDeployed = true;
        }
        lockCover.SetActive(false);
        emptyCover.SetActive(false);
    }

    public void InitializeItem(CharacterClass p_class, PLAYER_SKILL_TYPE p_skillType, bool p_isDeployed = false) {
        characterClass = p_class;
        playerSkillType = p_skillType;
        PlayerSkillData playerData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_skillType);
        txtUnlockPrice.text = unlockPrice.ToString();
        txtName.text = playerData.name;
        txtName.text = txtName.text.Replace("Demon ", "");
        txtHP.text = p_class.baseHP.ToString();
        txtAtk.text = p_class.baseAttackPower.ToString();
        txtAtkSpd.text = p_class.baseAttackSpeed.ToString();
        isMinion = true;
        imgPortrait.sprite = playerData.contextMenuIcon;
        if (!p_isDeployed) {
            isReadyForDeploy = true;
            isDeployed = false;
            txtStatus.text = "Ready";
        } else {
            txtStatus.text = "Deployed";
            isReadyForDeploy = false;
            isDeployed = true;
        }
        lockCover.SetActive(false);
        emptyCover.SetActive(false);
    }

    public void MakeSlotEmpty() {
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

    public void Deploy(Character p_createdCharacter = null) {
        deployedCharacter = p_createdCharacter;
        txtStatus.text = "Deployed";
        isDeployed = true;
        isReadyForDeploy = false;
    }
}