using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UtilityScripts;

[System.Serializable]
public class DeployedMonsterItemUI : MonoBehaviour {

    public Action<DeployedMonsterItemUI> onDelete;
    public Action<DeployedMonsterItemUI> onUnlockClicked;
    public Action onAddSummonClicked;

    public Button btnDelete;
    public Button btnItemClick;
    public Button btnUnlockSlot;
    public Button btnAddSummon;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public RuinarchText txtStatus;
    public RuinarchText txtSummonCost;
    public RuinarchText txtUnlockCost;
    public Image imgPortrait;

    public RuinarchText txtUnlockPrice;
    public GameObject lockCover;
    public GameObject emptyCover;
    public GameObject deadIcon;
    public GameObject addSummonCover;

    public bool isReadyForDeploy;
    public bool isDeployed;
    public bool isMinion;

    public int summonCost;
    public int unlockCost;

    public HoverText hoverText;

    private MonsterAndDemonUnderlingCharges _monsterOrMinion;
    public MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;
    public Character deployedCharacter;

    public MonsterAndDemonUnderlingCharges monsterUnderlingCharges;

    public GameObject manaIconAndPrice;
    private void OnEnable() {
        btnDelete.onClick.AddListener(OnDeleteClicked);
        btnItemClick.onClick.AddListener(OnItemClicked);
        btnUnlockSlot.onClick.AddListener(OnUnlockClicked);
        btnAddSummon.onClick.AddListener(OnAddSummonClicked);
	}

	private void OnDisable() {
        btnDelete.onClick.RemoveListener(OnDeleteClicked);
        btnItemClick.onClick.RemoveListener(OnItemClicked);
        btnUnlockSlot.onClick.RemoveListener(OnUnlockClicked);
        btnAddSummon.onClick.RemoveListener(OnAddSummonClicked);
    }

	public void InitializeItem(MonsterAndDemonUnderlingCharges p_underling, bool p_isDeployed = false, bool p_hideRemoveButton = false) {
        _monsterOrMinion = p_underling;
        CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(p_underling.characterClassName);
        txtName.text = cClass.className;
        txtHP.text = cClass.baseHP.ToString();
        txtAtk.text = cClass.baseAttackPower.ToString();
        txtAtkSpd.text = cClass.baseAttackSpeed.ToString();
        summonCost = CharacterManager.Instance.GetOrCreateCharacterClassData(cClass.className).GetSummonCost();
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
        addSummonCover.SetActive(false);
        lockCover.SetActive(false);
        emptyCover.SetActive(false);
        HideDeadIcon();
    }

    public void MakeSlotEmpty() {
        lockCover.SetActive(false);
        isDeployed = false;
        isReadyForDeploy = false;
        HideDeadIcon();
        emptyCover.SetActive(true);
        addSummonCover.SetActive(false);
    }

    public void ResetButton() {
        deployedCharacter = null;
        isDeployed = false;
        isReadyForDeploy = false;
        emptyCover.SetActive(true);
    }

    public void MakeSlotLocked(bool p_isAbleToBuy) {
        isDeployed = false;
        isReadyForDeploy = false;
        emptyCover.SetActive(false);
        lockCover.SetActive(true);
        addSummonCover.SetActive(false);
        txtUnlockCost.text = GetUnlockCost().ToString();
        btnUnlockSlot.gameObject.SetActive(true);
        HideDeadIcon();
        if (p_isAbleToBuy) {
            btnUnlockSlot.interactable = true;
            hoverText.SetText("Expand Capacity by 1");
        } else {
            btnUnlockSlot.interactable = false;
            hoverText.SetText("Not enough Chaotic Energy");
        }
    }

    public void MakeSlotLockedNoButton() {
        MakeSlotLocked(false);
        btnUnlockSlot.gameObject.SetActive(false);
    }

    public void EnableButton() {
        btnDelete.interactable = true;
        lockCover.SetActive(false);
    }

    void OnDeleteClicked() {
        onDelete?.Invoke(this);
    }

    void OnUnlockClicked() {
        onUnlockClicked?.Invoke(this);
    }

    void OnAddSummonClicked() {
        onAddSummonClicked?.Invoke();
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

    public void ShowDeadIcon() {
        deployedCharacter = null;
        isDeployed = true;
        deadIcon.SetActive(true);
    }

    public void HideDeadIcon() {
        deadIcon.SetActive(false);
    }

    public void DisplayAddSummon() {
        emptyCover.SetActive(false);
        lockCover.SetActive(false);
        btnUnlockSlot.gameObject.SetActive(false);
        addSummonCover.SetActive(true);
        HideDeadIcon();
        btnAddSummon.gameObject.SetActive(true);
    }

    public int GetUnlockCost() { 
        return SpellUtilities.GetModifiedSpellCost(unlockCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
    }
}