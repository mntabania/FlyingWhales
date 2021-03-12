using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class DefensePointUIController : MVCUIController, DefensePointUIView.IListener {

	#region MVCUI
	[SerializeField]
	private DefensePointUIModel m_defensePointUIModel;
	private DefensePointUIView m_defensePointUIView;

	bool m_isAllItemDeployed;
	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_defensePointUIView == null) {
			DefensePointUIView.Create(_canvas, m_defensePointUIModel, (p_ui) => {
				m_defensePointUIView = p_ui;
				m_defensePointUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				m_deployedSummonsUI = p_ui.UIModel.deployedItemSummonsUI;
				ListenToDeployedItems();
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
	private List<DeployedMonsterItemUI> m_deployedSummonsUI = new List<DeployedMonsterItemUI>();
	
	private PartyStructure m_targetPartyStructure;
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
		UnlistenToDeployedItems();
	}

	void ListenToDeployedItems() {
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked += OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked += OnUnlockClicked;
		});
	}

	void UnlistenToDeployedItems() {
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
		m_summonList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
		});
	}
	private void OnDefensePointClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init();
		}
	}

	public void Init() {
		InstantiateUI();
		InitializeSummons();
		InitializeDeployedItems();
		m_defensePointUIView.SetTitle("Defense Point");
		ProcessDeployButtonDisplay();
		GameManager.Instance.SetPausedState(true);
	}

	void HideDeployedItems() {
		int x = 0;
		for (; x < m_targetPartyStructure.maxSummonLimitDeployCount; ++x) {
			m_deployedSummonsUI[x].ShowRemoveButton();
			m_deployedSummonsUI[x].ResetButton();
			m_deployedSummonsUI[x].ShowManaCost();
			m_deployedSummonsUI[x].gameObject.SetActive(false);
		}
		m_defensePointUIView.ProcessSummonDisplay();
		m_targetPartyStructure.readyForDeployMinionCount = 0;
		m_targetPartyStructure.readyForDeploySummonCount = 0;
		m_targetPartyStructure.readyForDeployTargetCount = 0;
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetPartyStructure.deployedCSummonlass.Count; ++x) {
			m_deployedSummonsUI[x].gameObject.SetActive(true);
			m_deployedSummonsUI[x].HideManaCost();
			m_deployedSummonsUI[x].InitializeItem(m_targetPartyStructure.deployedCSummonlass[x], m_targetPartyStructure.deployedSummonSettings[x], m_targetPartyStructure.deployedSummonType[x], true, false);
		}
	}

	void InitializeDeployedItems() {
		HideDeployedItems();
		DisplayDeployedItems();
	}

	void HideSummonItems() {
		m_summonList.ForEach((eachItem) => {
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
				m_summonList[ctr++].InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges);
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
		if (!p_clickedItem.isMinion && m_targetPartyStructure.readyForDeploySummonCount + m_targetPartyStructure.deployedSummonCount < m_targetPartyStructure.maxSummonLimitDeployCount) {
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
			m_targetPartyStructure.readyForDeploySummonCount++;
			for (int x = 0; x < m_deployedSummonsUI.Count; ++x) {
				if (!m_deployedSummonsUI[x].isReadyForDeploy && !m_deployedSummonsUI[x].isDeployed) {
					m_deployedSummonsUI[x].gameObject.SetActive(true);
					m_deployedSummonsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
					break;
				}
			}
			m_defensePointUIView.ProcessSummonDisplay();
		}
		ProcessDeployButtonDisplay();
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		if (!p_itemUI.isMinion) {
			for (int x = 0; x < m_summonList.Count; ++x) {
				if (m_summonList[x].characterClass == p_itemUI.characterClass && (p_itemUI.isDeployed || p_itemUI.isReadyForDeploy)) {
					m_targetPartyStructure.readyForDeploySummonCount--;
					m_targetPartyStructure.RemoveItemOnRight(p_itemUI);
					if (p_itemUI.isDeployed) {
						PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(m_summonList[x].summonType, 1);
						m_targetPartyStructure.RemoveCharacterOnList(p_itemUI.deployedCharacter);
						p_itemUI.UndeployCharacter();
						p_itemUI.ShowManaCost();
					}
					p_itemUI.ResetButton();
					m_summonList[x].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
					p_itemUI.gameObject.SetActive(false);
				}
				m_defensePointUIView.ProcessSummonDisplay();
			}
		}
		ProcessDeployButtonDisplay();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetPartyStructure.maxSummonLimitDeployCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if (!m_isAllItemDeployed) {
			m_deployedSummonsUI.ForEach((eachSummonToBeDeployed) => {
				if (eachSummonToBeDeployed.isReadyForDeploy) {
					Summon summon = CharacterManager.Instance.CreateNewSummon(eachSummonToBeDeployed.summonType, FactionManager.Instance.GetFactionBasedOnName("Demon"), m_targetPartyStructure.currentSettlement); ;
					CharacterManager.Instance.PlaceSummonInitially(summon, m_targetPartyStructure.GetRandomTile());
					eachSummonToBeDeployed.HideManaCost();
					eachSummonToBeDeployed.Deploy(summon, true);
					m_targetPartyStructure.AddDeployedItem(eachSummonToBeDeployed);
					PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(eachSummonToBeDeployed.summonType, -1);
				}
			});
		} else {
			m_deployedSummonsUI.ForEach((eachSummonThatAreDployed) => {
				if (eachSummonThatAreDployed.isDeployed) {
					Summon summon = CharacterManager.Instance.CreateNewSummon(eachSummonThatAreDployed.summonType, FactionManager.Instance.GetFactionBasedOnName("Demon"), m_targetPartyStructure.currentSettlement); ;
					eachSummonThatAreDployed.ShowManaCost();
					eachSummonThatAreDployed.UndeployCharacter();
					eachSummonThatAreDployed.ResetButton();
					eachSummonThatAreDployed.gameObject.SetActive(false);
				}
			});
			m_targetPartyStructure.UnDeployAll();
			Init();
			m_defensePointUIView.ProcessSummonDisplay();
		}
		ProcessDeployButtonDisplay();
	}
	

	private void ProcessDeployButtonDisplay() {
		int displayedCount = 0;
		int deployedCount = m_targetPartyStructure.deployedSummonCount;
		m_deployedSummonsUI.ForEach((eachUi) => {
			if (eachUi.isReadyForDeploy) {
				displayedCount++;
			}
		});

		if (displayedCount > 0) {
			m_isAllItemDeployed = false;
			m_defensePointUIView.SetButtonDeployText("Deploy");
		} else if (deployedCount > 0 && displayedCount <= 0) {
			m_isAllItemDeployed = true;
			m_defensePointUIView.SetButtonDeployText("Undeploy");
		} else if (deployedCount >= 5) {
			m_isAllItemDeployed = true;
			m_defensePointUIView.SetButtonDeployText("Undeploy");
		} else {
			m_isAllItemDeployed = false;
			m_defensePointUIView.SetButtonDeployText("Deploy");
		}
	}

	public void OnCloseClicked() {
		HideSummonItems();
		HideUI();
		m_defensePointUIView.HideAllSubMenu();
		GameManager.Instance.SetPausedState(false);
	}

	public void OnAddSummonClicked() { m_defensePointUIView.ShowSummonSubContainer(); }
	
	public void OnCloseSummonSubContainer() { m_defensePointUIView.HideAllSubMenu(); }

	public void OnHoverOver() {
		if (m_isAllItemDeployed) {
			Tooltip.Instance.ShowSmallInfo("Disband the team.", "Undeploy team", autoReplaceText: false);
		} else {
			Tooltip.Instance.ShowSmallInfo("Send the team to do the quest.", "Deploy team", autoReplaceText: false);
		}
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
	}
	#endregion
}