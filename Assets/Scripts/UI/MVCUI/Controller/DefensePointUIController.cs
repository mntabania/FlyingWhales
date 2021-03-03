using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class DefensePointUIController : MVCUIController, DefensePointUIView.IListener {

	#region MVCUI
	[SerializeField]
	private DefensePointUIModel m_defensePointUIModel;
	private DefensePointUIView m_defensePointUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_defensePointUIView == null) {
			DefensePointUIView.Create(_canvas, m_defensePointUIModel, (p_ui) => {
				m_defensePointUIView = p_ui;
				m_defensePointUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				m_deployedMonsters = p_ui.UIModel.deployedMonsterItemUIs;
				m_deployedMonsters.ForEach((eachDeployedItem) => {
					eachDeployedItem.onClicked += OnDeployedMonsterClicked;
					eachDeployedItem.onUnlocked += OnUnlockClicked;
				});
				ShowUI();
			});
		} else {
			ShowUI();
		}
	}
	#endregion

	[SerializeField]
	private AvailableMonsterItemUI m_availableMonsterItemUI; //item to instantiate
	private List<AvailableMonsterItemUI> m_summonList = new List<AvailableMonsterItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedMonsters = new List<DeployedMonsterItemUI>();

	private DefensePoint m_targetDefensePointStructure;

	public int manaCostToDeploySummon = 10;

	private void Start() {
		UIManager.Instance.onDefensePointClicked += OnDefensePointClicked;
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onDefensePointClicked -= OnDefensePointClicked;
		}
		if (m_defensePointUIView != null) {
			m_defensePointUIView.Unsubscribe(this);
		}
		m_deployedMonsters.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
	}

	private void OnDefensePointClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetDefensePointStructure = p_clickedDefensePoint as DefensePoint;
			Init();
		}
	}

	public void Init() {
		InstantiateUI();
		InitializeSummons();
		InitializeDeployedItems();
	}

	void HideDeployedItems() {
		int x = 0;
		for (; x < m_targetDefensePointStructure.maxLimitDeployedCount; ++x) {
			m_deployedMonsters[x].MakeSlotEmpty();
		}
		for (; x < 5; ++x) {
			m_deployedMonsters[x].MakeSlotLocked();
		}
		m_targetDefensePointStructure.readyForDeployCount = 0;
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetDefensePointStructure.deployedClass.Count; ++x) {
			m_deployedMonsters[x].InitializeItem(m_targetDefensePointStructure.deployedClass[x], m_targetDefensePointStructure.deployedSettings[x], m_targetDefensePointStructure.deployedSummonType[x], true);
		}
	}

	void InitializeDeployedItems() {
		HideDeployedItems();
		DisplayDeployedItems();
	}

	void HideSommonItems() {
		m_summonList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
			eachItem.gameObject.SetActive(false);
		});
	}

	void InitializeSummons() {
		int ctr = 0;
		foreach (KeyValuePair<SUMMON_TYPE, MonsterUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges) {
			SummonSettings settings = CharacterManager.Instance.GetSummonSettings(entry.Key);
			CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
			if (ctr < m_summonList.Count) {
				m_summonList[ctr].gameObject.SetActive(true);
				m_summonList[ctr].onClicked += OnAvailableMonsterClicked;
				m_summonList[ctr++].InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon,  entry.Value.currentCharges, entry.Value.maxCharges);
			} else {
				AvailableMonsterItemUI summonItem = Instantiate(m_availableMonsterItemUI);
				summonItem.InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges);
				summonItem.transform.SetParent(m_defensePointUIView.GetAvailableSummonsParent());
				m_summonList.Add(summonItem);
				m_summonList[ctr++].onClicked += OnAvailableMonsterClicked;
			}
		}
	}

	void OnAvailableMonsterClicked(AvailableMonsterItemUI p_clickedItem) {
		if (m_targetDefensePointStructure.readyForDeployCount + m_targetDefensePointStructure.deployedCount < m_targetDefensePointStructure.maxLimitDeployedCount) {
			int price = manaCostToDeploySummon;
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < price);
			m_targetDefensePointStructure.readyForDeployCount++;

			for (int x = 0; x < m_deployedMonsters.Count; ++x) {
				if (!m_deployedMonsters[x].isReadyForDeploy && !m_deployedMonsters[x].isDeployed) {
					m_deployedMonsters[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
					break;
				}
			}
		}
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		m_targetDefensePointStructure.readyForDeployCount--;

		for (int x = 0; x < m_summonList.Count; ++x) {
			if (m_summonList[x].characterClass == p_itemUI.characterClass && (p_itemUI.isDeployed || p_itemUI.isReadyForDeploy)) {
				int price = manaCostToDeploySummon;
				if (p_itemUI.isDeployed) {
					PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[m_summonList[x].summonType].currentCharges++;
					m_targetDefensePointStructure.RemoveItemOnRight(p_itemUI);
				}
				m_summonList[x].AddOneCharge(PlayerManager.Instance.player.mana < price);
			}
		}
		p_itemUI.MakeSlotEmpty();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetDefensePointStructure.maxLimitDeployedCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		m_deployedMonsters.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				m_targetDefensePointStructure.AddDeployedItem(eachMonsterToBeDeployed);
				eachMonsterToBeDeployed.Deploy();
				PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[eachMonsterToBeDeployed.summonType].currentCharges--;
			}
		});
	}

	public void OnCloseClicked() {
		HideSommonItems();
		HideUI();
	}
	#endregion
}