using UnityEngine;
using Ruinarch.MVCFramework;
using System.Linq;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class MaraudUIController : MVCUIController, MaraudUIView.IListener {

	#region MVCUI
	[SerializeField]
	private MaraudUIModel m_maraudUIModel;
	private MaraudUIView m_maraudUIView;

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_maraudUIView == null) {
			MaraudUIView.Create(_canvas, m_maraudUIModel, (p_ui) => {
				m_maraudUIView = p_ui;
				m_maraudUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				m_deployedSummonsUI = p_ui.UIModel.deployedItemSummonsUI;
				m_deployedMinionsUI = p_ui.UIModel.deployedItemMinionsUI;
				m_deployedMinionsUI.ForEach((eachDeployedItem) => {
					eachDeployedItem.onClicked += OnDeployedMonsterClicked;
					eachDeployedItem.onUnlocked += OnUnlockClicked;
				});
				m_deployedSummonsUI.ForEach((eachDeployedItem) => {
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
	private List<AvailableMonsterItemUI> m_minionList = new List<AvailableMonsterItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedSummonsUI = new List<DeployedMonsterItemUI>();
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedMinionsUI = new List<DeployedMonsterItemUI>();

	private Maraud m_targetMaraudStructure;

	public int manaCostToDeploySummon = 10;

	private void Start() {
		UIManager.Instance.onMaraudClicked += OnMaraudClicked;
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onMaraudClicked -= OnMaraudClicked;
		}
		if (m_maraudUIView != null) {
			m_maraudUIView.Unsubscribe(this);
		}
		m_deployedMinionsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
	}

	private void OnMaraudClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetMaraudStructure = p_clickedDefensePoint as Maraud;
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
		for (; x < m_targetMaraudStructure.maxLimitDeployedCount; ++x) {
			m_deployedSummonsUI[x].MakeSlotEmpty();
		}
		for (; x < 4; ++x) {
			m_deployedSummonsUI[x].MakeSlotLocked();
		}
		m_targetMaraudStructure.readyForDeployCount = 0;
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetMaraudStructure.deployedClass.Count; ++x) {
			m_deployedSummonsUI[x].InitializeItem(m_targetMaraudStructure.deployedClass[x], m_targetMaraudStructure.deployedSettings[x], m_targetMaraudStructure.deployedSummonType[x], true);
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
				m_summonList[ctr++].InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges);
			} else {
				AvailableMonsterItemUI summonItem = Instantiate(m_availableMonsterItemUI);
				summonItem.InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges);
				summonItem.transform.SetParent(m_maraudUIView.GetAvailableSummonsParent());
				m_summonList.Add(summonItem);
				m_summonList[ctr++].onClicked += OnAvailableMonsterClicked;
			}
		}
	}

	void OnAvailableMonsterClicked(AvailableMonsterItemUI p_clickedItem) {
		if (m_targetMaraudStructure.readyForDeployCount + m_targetMaraudStructure.deployedCount < m_targetMaraudStructure.maxLimitDeployedCount) {
			int price = manaCostToDeploySummon;
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < price);
			m_targetMaraudStructure.readyForDeployCount++;

			for (int x = 0; x < m_deployedSummonsUI.Count; ++x) {
				if (!m_deployedSummonsUI[x].isReadyForDeploy && !m_deployedSummonsUI[x].isDeployed) {
					m_deployedSummonsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
					break;
				}
			}
		}
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		m_targetMaraudStructure.readyForDeployCount--;

		for (int x = 0; x < m_summonList.Count; ++x) {
			if (m_summonList[x].characterClass == p_itemUI.characterClass && (p_itemUI.isDeployed || p_itemUI.isReadyForDeploy)) {
				int price = manaCostToDeploySummon;
				if (p_itemUI.isDeployed) {
					PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[m_summonList[x].summonType].currentCharges++;
					m_targetMaraudStructure.RemoveItemOnRight(p_itemUI);
				}
				m_summonList[x].AddOneCharge(PlayerManager.Instance.player.mana < price);
			}
		}
		p_itemUI.MakeSlotEmpty();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetMaraudStructure.maxLimitDeployedCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		m_deployedSummonsUI.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				m_targetMaraudStructure.AddDeployedItem(eachMonsterToBeDeployed);
				eachMonsterToBeDeployed.Deploy();
				PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[eachMonsterToBeDeployed.summonType].currentCharges--;
			}
		});
	}

	public void OnCloseClicked() {
		HideSommonItems();
		HideUI();
	}

	public void OnSummonsClicked(bool isOn) {
		if (isOn) {
			m_maraudUIView.ShowSummonTab();
		}
	}
	public void OnMinionClicked(bool isOn) {
		if (isOn) {
			m_maraudUIView.ShowMinionTab();
		}
	}
	#endregion
}