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
	private MonsterUnderlingQuantityNameplateItem m_availableMonsterItemUI; //item to instantiate
	private List<MonsterUnderlingQuantityNameplateItem> m_summonList = new List<MonsterUnderlingQuantityNameplateItem>();
	
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

	void ReturnAllItemToPool() {
		for (int x = 0; x < m_summonList.Count; ++x) {
			ObjectPoolManager.Instance.DestroyObject(m_summonList[x]);
		};
		m_summonList.Clear();
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
	}
	private void OnDefensePointClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init();
		}
	}
	public void Init() {
		m_targetPartyStructure.InitializeTeam();
		ReturnAllItemToPool();
		InstantiateUI();
		InitializeSummons();
		InitializeDeployedItems();
		m_defensePointUIView.SetTitle("Defense Point");
		ProcessDeployButtonDisplay();
		UIManager.Instance.Pause();
	}

	void HideDeployedItems() {
		int x = 0;
		for (; x < m_targetPartyStructure.partyData.maxSummonLimitDeployCount; ++x) {
			m_deployedSummonsUI[x].ShowRemoveButton();
			m_deployedSummonsUI[x].ResetButton();
			m_deployedSummonsUI[x].ShowManaCost();
			m_deployedSummonsUI[x].gameObject.SetActive(false);
		}
		m_defensePointUIView.ProcessSummonDisplay();
		m_targetPartyStructure.partyData.ResetAllReadyCounts();
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetPartyStructure.partyData.deployedSummonUnderlings.Count; ++x) {
			m_deployedSummonsUI[x].gameObject.SetActive(true);
			m_deployedSummonsUI[x].HideManaCost();
			m_targetPartyStructure.partyData.readyForDeploySummonCount++;
			m_deployedSummonsUI[x].InitializeItem(m_targetPartyStructure.partyData.deployedSummonUnderlings[x], true, false);
			m_deployedSummonsUI[x].Deploy(m_targetPartyStructure.partyData.deployedSummons[x], true);
		}
		m_defensePointUIView.ProcessSummonDisplay();
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
		foreach (KeyValuePair<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges) {
			GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_defensePointUIView.GetAvailableSummonsParent());
			MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
			item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
			item.SetObject(entry.Value);
			item.SetAsButton();
			m_summonList.Add(item);
		}
		m_defensePointUIView.ProcessSummonDisplay();
	}

	void OnAvailableMonsterClicked(MonsterAndDemonUnderlingCharges p_clickedMonster, MonsterUnderlingQuantityNameplateItem p_item) {
		if (!p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeploySummonCount < m_targetPartyStructure.partyData.maxSummonLimitDeployCount) {
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedSummonsUI, p_clickedMonster);
			m_defensePointUIView.ProcessSummonDisplay();
		}
		ProcessDeployButtonDisplay();
	}

	void ProcessDeployedItemFromClickingAvailableItem(List<DeployedMonsterItemUI> deployedItemList, MonsterAndDemonUnderlingCharges p_monsterClicked) {
		for (int x = 0; x < deployedItemList.Count; ++x) {
			if (!deployedItemList[x].isReadyForDeploy && !deployedItemList[x].isDeployed) {
				deployedItemList[x].gameObject.SetActive(true);
				deployedItemList[x].InitializeItem(p_monsterClicked);
				deployedItemList[x].ShowManaCost();
				deployedItemList[x].ShowRemoveButton();
				m_targetPartyStructure.partyData.readyForDeploySummonCount++;
				break;
			}
		}
	}

	void ProcessAvailableItemFromClickingDeployedItem(List<MonsterUnderlingQuantityNameplateItem> availItems, DeployedMonsterItemUI p_itemUI) {
		availItems.ForEach((availableSummons) => {
			if (availableSummons.obj.characterClass == p_itemUI.obj.characterClass) {
				availableSummons.IncreaseOneChargeForDisplayPurpose();
				if (p_itemUI.isDeployed) {
					m_targetPartyStructure.RemoveCharacterOnList(p_itemUI.deployedCharacter);
				}
				p_itemUI.ResetButton();
				p_itemUI.gameObject.SetActive(false);
				m_targetPartyStructure.partyData.readyForDeploySummonCount--;
			}
		});
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		if (!p_itemUI.isMinion) {
			ProcessAvailableItemFromClickingDeployedItem(m_summonList, p_itemUI);
			m_defensePointUIView.ProcessSummonDisplay();
		} 
		ProcessDeployButtonDisplay();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetPartyStructure.partyData.maxSummonLimitDeployCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if (!m_isAllItemDeployed) {
			int newDeployedCount = 0;
			m_deployedSummonsUI.ForEach((eachSummonToBeDeployed) => {
				if (eachSummonToBeDeployed.isReadyForDeploy) {
					Summon summon = CharacterManager.Instance.CreateNewSummon(eachSummonToBeDeployed.obj.monsterType, PlayerManager.Instance.player.playerFaction, m_targetPartyStructure.currentSettlement);
					CharacterManager.Instance.PlaceSummonInitially(summon, m_targetPartyStructure.GetRandomTile());
					summon.OnSummonAsPlayerMonster();
					eachSummonToBeDeployed.HideManaCost();
					eachSummonToBeDeployed.Deploy(summon, true);
					m_targetPartyStructure.AddDeployedItem(eachSummonToBeDeployed);
					PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(eachSummonToBeDeployed.obj.monsterType, -1);
					newDeployedCount++;
				}
			});
			if (newDeployedCount > 0) {
				m_targetPartyStructure.DeployParty();
			}
			
		} else {
			m_deployedSummonsUI.ForEach((eachSummonThatAreDployed) => {
				if (eachSummonThatAreDployed.isDeployed) {
					//Why create summon here?
					//Summon summon = CharacterManager.Instance.CreateNewSummon(eachSummonThatAreDployed.summonType, FactionManager.Instance.GetFactionBasedOnName("Demon"), m_targetPartyStructure.currentSettlement);
					eachSummonThatAreDployed.ShowManaCost();
					eachSummonThatAreDployed.UndeployCharacter();
					eachSummonThatAreDployed.ResetButton();
					eachSummonThatAreDployed.gameObject.SetActive(false);
				}
			});
			m_targetPartyStructure.ResetExistingCharges();
			m_targetPartyStructure.UnDeployAll();
			Init();
			m_defensePointUIView.ProcessSummonDisplay();
		}
		ProcessDeployButtonDisplay();
	}
	

	private void ProcessDeployButtonDisplay() {
		int displayedCount = 0;
		int deployedCount = m_targetPartyStructure.partyData.deployedSummonCount;
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
		ReturnAllItemToPool();
		HideUI();
		m_defensePointUIView.HideAllSubMenu();
		UIManager.Instance.ResumeLastProgressionSpeed();
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