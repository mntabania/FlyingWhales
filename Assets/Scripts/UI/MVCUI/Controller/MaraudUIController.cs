﻿using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using System.Collections.Generic;
using UnityEngine;

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
				m_deployedTargetItemUI = p_ui.UIModel.deployedTargetItemUI;
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
	[SerializeField] 
	private AvailableTargetItemUI m_availableTargetItemUI; //item to instantiate
	private List<MonsterUnderlingQuantityNameplateItem> m_summonList = new List<MonsterUnderlingQuantityNameplateItem>();
	private List<MonsterUnderlingQuantityNameplateItem> m_minionList = new List<MonsterUnderlingQuantityNameplateItem>();
	private List<AvailableTargetItemUI> m_targetList = new List<AvailableTargetItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedSummonsUI = new List<DeployedMonsterItemUI>();
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedMinionsUI = new List<DeployedMonsterItemUI>();
	[SerializeField]
	private List<DeployedTargetItemUI> m_deployedTargetItemUI = new List<DeployedTargetItemUI>();

	private int m_totalDeployCost = 0;

	private PartyStructure m_targetPartyStructure;
	private bool m_isTeamDeployed;

	private void Start() {
		UIManager.Instance.onMaraudClicked += OnMaraudClicked;
		UIManager.Instance.onKennelClicked += OnKennelClicked;
		UIManager.Instance.onTortureChamberClicked += OnTortureChambersClicked;
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onMaraudClicked -= OnMaraudClicked;
			UIManager.Instance.onKennelClicked -= OnKennelClicked;
			UIManager.Instance.onTortureChamberClicked -= OnTortureChambersClicked;
		}
		if (m_maraudUIView != null) {
			m_maraudUIView.Unsubscribe(this);
		}
		UnlistenToDeployedItems();
	}

	void ListenToDeployedItems() {
		m_deployedMinionsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete += OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete += OnDeployedMonsterClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDeleteClick += OnDeployedTargetClicked;
		});
	}

	void UnlistenToDeployedItems() {
		m_deployedMinionsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete -= OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete -= OnDeployedMonsterClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDeleteClick -= OnDeployedTargetClicked;
		});
		m_targetList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableTargetClicked;
		});
	}
	private void OnMaraudClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init("Raid Party");
		}
	}

	private void OnKennelClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init("Snatch Party");
		}
	}

	private void OnTortureChambersClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init("Snatch Party");
		}
	}

	public void Init(string p_title = "") {
		m_totalDeployCost = 0;
		m_targetPartyStructure.InitializeTeam();
		ReturnAllItemToPool();
		InstantiateUI();
		InitializeSummons();
		InitializeMinions();
		InitializeTargets();
		InitializeDeployedItems();
		if (m_isTeamDeployed) {
			m_maraudUIView.SetButtonDeployText("Undeploy");
		} else {
			m_maraudUIView.SetButtonDeployText("Deploy");
		}
		ProcessButtonAvailability();
		UIManager.Instance.Pause();
	}

	void HideDeployedItems() {
		int x = 0;
		for (; x < m_targetPartyStructure.partyData.maxSummonLimitDeployCount; ++x) {
			m_deployedSummonsUI[x].ShowRemoveButton();
			m_deployedSummonsUI[x].ResetButton();
			m_deployedSummonsUI[x].gameObject.SetActive(false);
		}
		m_deployedMinionsUI[0].ShowRemoveButton();
		m_deployedMinionsUI[0].ResetButton();
		m_deployedTargetItemUI[0].ShowRemoveButton();
		m_deployedTargetItemUI[0].ResetButton();
		m_maraudUIView.ShowMinionButtonHideMinionContainer();
		m_maraudUIView.ShowTargetButtonHideTargetContainer();
		m_maraudUIView.ProcessSummonDisplay();
		m_targetPartyStructure.partyData.ResetAllReadyCounts();
	}

	void DisplayDeployedItems() {
		if ((m_targetPartyStructure.partyData.deployedMinionCount > 0 || m_targetPartyStructure.partyData.deployedSummonCount > 0) && (m_targetPartyStructure.partyData.deployedTargetCount > 0)) {
			m_isTeamDeployed = true;
		} else {
			m_isTeamDeployed = false;
		}
		for (int x = 0; x < m_targetPartyStructure.partyData.deployedSummonUnderlings.Count; ++x) {
			m_deployedSummonsUI[x].gameObject.SetActive(true);
			m_deployedSummonsUI[x].InitializeItem(m_targetPartyStructure.partyData.deployedSummonUnderlings[x], true, true);
		}
		if (m_targetPartyStructure.partyData.deployedMinionCount > 0) {
			m_maraudUIView.HideMinionButtonShowMinionContainer();
			m_deployedMinionsUI[0].InitializeItem(m_targetPartyStructure.partyData.deployedMinionUnderlings[0], true, true);
		} else {
			m_maraudUIView.ShowMinionButtonHideMinionContainer();
		}

		if (m_targetPartyStructure.partyData.deployedTargetCount > 0) {
			m_maraudUIView.HideTargetButtonShowTargetContainer();
			m_deployedTargetItemUI[0].InitializeItem(m_targetPartyStructure.partyData.deployedTargets[0], true);
		} else {
			m_maraudUIView.ShowTargetButtonHideTargetContainer();
		}
	}

	void InitializeDeployedItems() {
		HideDeployedItems();
		DisplayDeployedItems();
	}

	void ReturnAllItemToPool() { 
		for(int x = 0; x < m_summonList.Count; ++x) {
			ObjectPoolManager.Instance.DestroyObject(m_summonList[x]);
		};
		for (int x = 0; x < m_minionList.Count; ++x) {
			ObjectPoolManager.Instance.DestroyObject(m_minionList[x]);
		};
		m_summonList.Clear();
		m_minionList.Clear();
	}

	void HideAvailableItems() {
		m_targetList.ForEach((eachItem) => {
			eachItem.gameObject.SetActive(false);
		});
	}

	void InitializeSummons() {
		foreach (KeyValuePair<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges) {
			GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_maraudUIView.GetAvailableSummonsParent());
			MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
			item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
			item.SetObject(entry.Value);
			item.SetAsButton();
			m_summonList.Add(item);
			if (PlayerManager.Instance.player.mana < item.summonCost) {
				item.SetInteractableState(false);
			} else {
				item.SetInteractableState(true);
			}
			item.AddHoverEnterAction(OnHoverItemOccupiedStructure);
			item.AddHoverExitAction(OnHoverExitItemOccupiedStructure);
		}
		m_maraudUIView.ProcessSummonDisplay();
	}

	void OnHoverItemOccupiedStructure(MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges) {
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			UIManager.Instance.ShowSmallInfo("You cant add a team member bacause the structure is occupied", "Structure Occupied", true);
		}
	}

	void OnHoverExitItemOccupiedStructure(MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges) {
		UIManager.Instance.HideSmallInfo();
	}

	void InitializeTargets() {
		int ctr = 0;
		m_targetPartyStructure.allPossibleTargets.ForEach((EachTarget) => {
			if (ctr < m_targetList.Count) {
				m_targetList[ctr].gameObject.SetActive(true);
				m_targetList[ctr].InitializeItem(EachTarget);
				SetTargetHoverText(m_targetList[ctr++]);
			} else {
				AvailableTargetItemUI availableTargetItemUI = Instantiate(m_availableTargetItemUI);
				availableTargetItemUI.InitializeItem(EachTarget);
				availableTargetItemUI.transform.SetParent(m_maraudUIView.GetAvailableTargetParent());
				m_targetList.Add(availableTargetItemUI);
				SetTargetHoverText(m_targetList[ctr]);
				m_targetList[ctr++].onClicked += OnAvailableTargetClicked;
			}
		});
	}

	void SetTargetHoverText(AvailableTargetItemUI p_item) {
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			p_item.SetHoverText("You cant add a team member bacause the structure is occupied");
		} else {
			p_item.SetHoverText("Target already chased by another party");
		}
	}

	void InitializeMinions() {
		foreach (KeyValuePair<MINION_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.demonUnderlingCharges) {
			MinionSettings settings = CharacterManager.Instance.GetMinionSettings(entry.Value.minionType);
			CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className); 
			SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(entry.Value.minionType);
			if (skillData.isInUse) {
				GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_maraudUIView.GetAvailableMinionsParent());
				MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
				item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
				item.SetObject(entry.Value);
				item.SetAsButton();
				m_minionList.Add(item);
				if (PlayerManager.Instance.player.mana < item.summonCost) {
					item.SetInteractableState(false);
				} else {
					item.SetInteractableState(true);
				}
				item.AddHoverEnterAction(OnHoverItemOccupiedStructure);
				item.AddHoverExitAction(OnHoverExitItemOccupiedStructure);
			}
		}
	}
	void OnAvailableTargetClicked(AvailableTargetItemUI p_itemUI) {
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			return;
		}
		bool exitLoop = false;
		for (int x = 0; x < m_deployedTargetItemUI.Count && !exitLoop; ++x) {
			if (m_deployedTargetItemUI[x].isReadyForDeploy) {
				for (int y = 0; y < m_targetList.Count; ++y) {
					if (m_targetList[y].target == m_deployedTargetItemUI[x].target) {
						m_deployedTargetItemUI[x].InitializeItem(p_itemUI.target);
						exitLoop = true;
						m_maraudUIView.HideTargetButtonShowTargetContainer();
						m_targetPartyStructure.partyData.readyForDeployTargetCount++;
						break;
					}
				}
			} else if (!m_deployedTargetItemUI[x].isDeployed && !m_deployedTargetItemUI[x].isReadyForDeploy) {
				m_targetPartyStructure.partyData.readyForDeployTargetCount++;
				m_deployedTargetItemUI[x].InitializeItem(p_itemUI.target);
				m_maraudUIView.HideTargetButtonShowTargetContainer();
				break;
			}
		}
		ProcessButtonAvailability();
	}

	void OnAvailableMonsterClicked(MonsterAndDemonUnderlingCharges p_clickedMonster, MonsterUnderlingQuantityNameplateItem p_item) {
		if (m_isTeamDeployed) {
			return;
		}
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			return;
		}
		if (!p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeploySummonCount < m_targetPartyStructure.partyData.maxSummonLimitDeployCount) {
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedSummonsUI, p_clickedMonster);
			m_maraudUIView.ProcessSummonDisplay();
		} else if (p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0) {
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedMinionsUI, p_clickedMonster);
			m_maraudUIView.HideMinionButtonShowMinionContainer();
		}
		ProcessButtonAvailability();
	}

	void ProcessDeployedItemFromClickingAvailableItem(List<DeployedMonsterItemUI> deployedItemList, MonsterAndDemonUnderlingCharges p_monsterClicked) {
		for (int x = 0; x < deployedItemList.Count; ++x) {
			if (!deployedItemList[x].isReadyForDeploy) {
				deployedItemList[x].gameObject.SetActive(true);
				if (p_monsterClicked.isDemon) {
					m_targetPartyStructure.partyData.readyForDeployMinionCount++;
					deployedItemList[x].InitializeItem(p_monsterClicked);
					m_totalDeployCost += deployedItemList[x].summonCost;
				} else {
					deployedItemList[x].InitializeItem(p_monsterClicked);
					m_targetPartyStructure.partyData.readyForDeploySummonCount++;
					m_totalDeployCost += deployedItemList[x].summonCost;
				}
				break;
			}
		}
	}

	void ProcessAvailableItemFromClickingDeployedItem(List<MonsterUnderlingQuantityNameplateItem> availItems, DeployedMonsterItemUI p_itemUI) {
		availItems.ForEach((availableSummons) => {
			if (availableSummons.obj.characterClass == p_itemUI.obj.characterClass) {
				availableSummons.IncreaseOneChargeForDisplayPurpose();
				p_itemUI.ResetButton();
				p_itemUI.gameObject.SetActive(false);
				if (availableSummons.obj.isDemon) {
					m_totalDeployCost -= p_itemUI.summonCost;
					m_targetPartyStructure.partyData.readyForDeployMinionCount--;
				} else {
					m_totalDeployCost -= p_itemUI.summonCost;
					m_targetPartyStructure.partyData.readyForDeploySummonCount--;
				}
			}
		});
	}

	void ProcessButtonAvailability() {
		if (!m_isTeamDeployed) {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0 && m_targetPartyStructure.partyData.readyForDeployTargetCount > 0) {
				m_maraudUIView.EnableDeployButton();
			} else if (!m_targetPartyStructure.IsAvailableForTargeting()) {
				m_maraudUIView.DisableDeployButton();
			} else if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
				m_maraudUIView.DisableDeployButton();
			} else {
				m_maraudUIView.DisableDeployButton();
			}
		} else {
			m_maraudUIView.EnableDeployButton();
		}
	}

	void OnDeployedTargetClicked(DeployedTargetItemUI p_itemUI) {
		if (m_isTeamDeployed) {
			return;
		}
		for (int x = 0; x < m_targetList.Count; ++x) {
			if (m_targetList[x].target == p_itemUI.target && (p_itemUI.isReadyForDeploy)) {
				m_maraudUIView.ShowTargetButtonHideTargetContainer();
				m_targetPartyStructure.partyData.readyForDeployTargetCount--;
			}
		}
		ProcessButtonAvailability();
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		if (m_isTeamDeployed) {
			return;
		}
		if (!p_itemUI.isMinion) {
			ProcessAvailableItemFromClickingDeployedItem(m_summonList, p_itemUI);
			m_maraudUIView.ProcessSummonDisplay();
		} else {
			ProcessAvailableItemFromClickingDeployedItem(m_minionList, p_itemUI);
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0) {
				m_maraudUIView.ShowMinionButtonHideMinionContainer();
			}
		}
		ProcessButtonAvailability();
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if ((m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0 || m_targetPartyStructure.partyData.readyForDeployTargetCount <= 0) && !m_isTeamDeployed) {
			return; //TODO: MESSAGE PLAYER THAT HE NEEDS LEADER
		}
		if (!m_isTeamDeployed) {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Deploy Party.", $"Are you sure you want to use {m_totalDeployCost.ToString()} mana and deploy the party?", OnYesDeploy, showCover: true, layer: 150);
		} else {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Disband Party.", $"Are you sure you want to disband the party?", OnYesUndeploy, showCover: true, layer: 150);
		}
	}

	void OnYesDeploy() {
		LocationStructure structureToBePlaced = m_targetPartyStructure;
		if (!structureToBePlaced.structureType.IsOpenSpace()) {
			structureToBePlaced = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		}
		m_deployedSummonsUI.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				Summon summon = CharacterManager.Instance.CreateNewSummon(eachMonsterToBeDeployed.obj.monsterType, PlayerManager.Instance.player.playerFaction, m_targetPartyStructure.currentSettlement, bypassIdeologyChecking: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, structureToBePlaced.GetRandomPassableTile());
				summon.OnSummonAsPlayerMonster();
				summon.SetDeployedAtStructure(m_targetPartyStructure);
				eachMonsterToBeDeployed.Deploy(summon);
				m_targetPartyStructure.AddDeployedItem(eachMonsterToBeDeployed);
				PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(eachMonsterToBeDeployed.obj.monsterType, -1);
			}
		});
		if (m_deployedMinionsUI[0].isReadyForDeploy && m_deployedMinionsUI[0].isMinion) {
			SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(m_deployedMinionsUI[0].obj.minionType);
			Character minion = null;
			skillData.ActivateAbility(structureToBePlaced.GetRandomPassableTile(), ref minion);
			minion.SetDeployedAtStructure(m_targetPartyStructure);
			m_deployedMinionsUI[0].Deploy(minion);
			m_targetPartyStructure.AddDeployedItem(m_deployedMinionsUI[0]);
		}

		if (m_deployedTargetItemUI[0].isReadyForDeploy) {
			m_deployedTargetItemUI[0].Deploy();
			m_targetPartyStructure.AddTargetOnCurrentList(m_deployedTargetItemUI[0].target);
		}
		m_targetPartyStructure.DeployParty();
		m_isTeamDeployed = true;
		m_maraudUIView.SetButtonDeployText("Undeploy");
		PlayerManager.Instance.player.AdjustMana(m_totalDeployCost);
		m_totalDeployCost = 0;
		OnCloseClicked();
	}

	void OnYesUndeploy() {
		m_isTeamDeployed = false;
		m_targetPartyStructure.ResetExistingCharges();
		m_targetPartyStructure.UnDeployAll();
		m_deployedSummonsUI.ForEach((eachSummon) => {
			m_targetPartyStructure.RemoveItemOnRight(eachSummon);
			eachSummon.UndeployCharacter();
			eachSummon.ResetButton();
		});
		m_deployedMinionsUI.ForEach((eachMinion) => {
			m_targetPartyStructure.RemoveItemOnRight(eachMinion);
			eachMinion.UndeployCharacter();
			eachMinion.ResetButton();
		});
		m_deployedTargetItemUI.ForEach((eachItem) => {
			eachItem.UndeployCharacter();
			eachItem.ResetButton();
			eachItem.ShowRemoveButton();
		});
		m_maraudUIView.ShowMinionButtonHideMinionContainer();
		m_maraudUIView.ShowTargetButtonHideTargetContainer();
		m_maraudUIView.ProcessSummonDisplay();
		Init();
	}

	public void OnCloseClicked() {
		HideAvailableItems();
		ReturnAllItemToPool();
		HideUI();
		m_maraudUIView.HideAllSubMenu();
		UIManager.Instance.ResumeLastProgressionSpeed();
	}

	public void OnAddSummonClicked() { m_maraudUIView.ShowSummonSubContainer(); }
	public void OnAddMinionClicked() { m_maraudUIView.ShowMinionSubContainer(); }
	public void OnAddTargetClicked() { m_maraudUIView.ShowTargetSubContainer(); }

	public void OnCloseSummonSubContainer() { m_maraudUIView.HideAllSubMenu(); }
	public void OnCloseMinionSubContainer() { m_maraudUIView.HideAllSubMenu(); }
	public void OnCloseTargetSubContainer() { m_maraudUIView.HideAllSubMenu(); }

	public void OnHoverOver() {
		if (m_isTeamDeployed) {
			Tooltip.Instance.ShowSmallInfo("Disband the team.", "Undeploy team", autoReplaceText: false);
		} else {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0 && m_targetPartyStructure.partyData.readyForDeployTargetCount > 0) {
				Tooltip.Instance.ShowSmallInfo("Send the team to do the quest.", "Deploy team", autoReplaceText: false);
			} else if (!m_targetPartyStructure.IsAvailableForTargeting()) {
				Tooltip.Instance.ShowSmallInfo("Can't build team, structure is occupied.", "Occupied Structure", autoReplaceText: false);
			} else if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
				Tooltip.Instance.ShowSmallInfo("Can't build team, Not enough Mana", "Not enough Mana", autoReplaceText: false);
			} else {
				Tooltip.Instance.ShowSmallInfo("Should atleast have 1 target and 1 leader", "Deploy team", autoReplaceText: false);
			}
		}
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
	}
	#endregion
}