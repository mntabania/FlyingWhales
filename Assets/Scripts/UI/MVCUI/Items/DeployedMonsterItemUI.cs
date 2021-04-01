using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class DeployedMonsterItemUI : MonoBehaviour {

    public Action<DeployedMonsterItemUI> onDelete;

    public Button btnDelete;
    public Button btnItemClick;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public RuinarchText txtStatus;
    public RuinarchText txtSummonCost;
    public Image imgPortrait;

    public RuinarchText txtUnlockPrice;
    public GameObject lockCover;
    public GameObject emptyCover;

    public bool isReadyForDeploy;
    public bool isDeployed;
    public bool isMinion;

    public int summonCost;

    private MonsterAndDemonUnderlingCharges _monsterOrMinion;
    public MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;
    public Character deployedCharacter;

    public MonsterAndDemonUnderlingCharges monsterUnderlingCharges;

    public GameObject manaIconAndPrice;
    private void OnEnable() {
        btnDelete.onClick.AddListener(OnDeleteClicked);
        btnItemClick.onClick.AddListener(OnItemClicked);
	}

	private void OnDisable() {
        btnDelete.onClick.RemoveListener(OnDeleteClicked);
        btnItemClick.onClick.RemoveListener(OnItemClicked);
    }

	public void InitializeItem(MonsterAndDemonUnderlingCharges p_underling, bool p_isDeployed = false, bool p_hideRemoveButton = false) {
        _monsterOrMinion = p_underling;
        txtName.text = p_underling.characterClass.className;
        txtHP.text = p_underling.characterClass.baseHP.ToString();
        txtAtk.text = p_underling.characterClass.baseAttackPower.ToString();
        txtAtkSpd.text = p_underling.characterClass.baseAttackSpeed.ToString();
        summonCost = CharacterManager.Instance.GetOrCreateCharacterClassData(_monsterOrMinion.characterClass.className).summonCost;
        txtSummonCost.text = summonCost.ToString();
        if (p_underling.isDemon) {
            isMinion = true;
            imgPortrait.sprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetMinionSettings(p_underling.minionType).className).portraitSprite;
        } else {
            isMinion = false;
            imgPortrait.sprite = CharacterManager.Instance.GetOrCreateCharacterClassData(CharacterManager.Instance.GetSummonSettings(p_underling.monsterType).className).portraitSprite;
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
        btnDelete.interactable = true;
        lockCover.SetActive(false);
    }

    void OnDeleteClicked() {
        onDelete?.Invoke(this);
    }
    
    void OnItemClicked() {
        if (deployedCharacter != null) {
            UIManager.Instance.OpenObjectUI(deployedCharacter);
        }
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
        btnDelete.gameObject.SetActive(false);
    }

    public void ShowRemoveButton() {
        btnDelete.gameObject.SetActive(true);
    }
}