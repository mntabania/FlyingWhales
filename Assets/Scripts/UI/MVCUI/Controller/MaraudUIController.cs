using Inner_Maps.Location_Structures;
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
	private AvailableMonsterItemUI m_availableMonsterItemUI; //item to instantiate
	[SerializeField] 
	private AvailableTargetItemUI m_availableTargetItemUI; //item to instantiate
	private List<AvailableMonsterItemUI> m_summonList = new List<AvailableMonsterItemUI>();
	private List<AvailableMonsterItemUI> m_minionList = new List<AvailableMonsterItemUI>();
	private List<AvailableTargetItemUI> m_targetList = new List<AvailableTargetItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedSummonsUI = new List<DeployedMonsterItemUI>();
	[SerializeField]
	private List<DeployedMonsterItemUI> m_deployedMinionsUI = new List<DeployedMonsterItemUI>();
	[SerializeField]
	private List<DeployedTargetItemUI> m_deployedTargetItemUI = new List<DeployedTargetItemUI>();
		
	public int manaCostToDeploySummon = 10;

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
			eachDeployedItem.onClicked += OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked += OnUnlockClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked += OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked += OnUnlockClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked += OnDeployedTargetClicked;
		});
	}

	void UnlistenToDeployedItems() {
		m_deployedMinionsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedMonsterClicked;
			eachDeployedItem.onUnlocked -= OnUnlockClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onClicked -= OnDeployedTargetClicked;
		});

		m_summonList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
		});
		m_minionList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
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
		m_targetPartyStructure.InitializeTeam();
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
		if (p_title != string.Empty) {
			m_maraudUIView.SetTitle(p_title);
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
		for (int x = 0; x < m_targetPartyStructure.partyData.deployedCSummonlass.Count; ++x) {
			m_deployedSummonsUI[x].gameObject.SetActive(true);
			m_deployedSummonsUI[x].InitializeItem(m_targetPartyStructure.partyData.deployedCSummonlass[x], m_targetPartyStructure.partyData.deployedSummonSettings[x], (m_targetPartyStructure.partyData.deployedSummons[x] as Summon).summonType, true, true);
		}
		if (m_targetPartyStructure.partyData.deployedMinionCount > 0) {
			m_maraudUIView.HideMinionButtonShowMinionContainer();
			m_deployedMinionsUI[0].InitializeItem(m_targetPartyStructure.partyData.deployedMinionClass[0], m_targetPartyStructure.partyData.deployedMinionsSkillType[0], true);
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

	void HideAvailableItems() {
		m_summonList.ForEach((eachItem) => {
			eachItem.gameObject.SetActive(false);
		});
		m_minionList.ForEach((eachItem) => {
			eachItem.gameObject.SetActive(false);
		});
		m_targetList.ForEach((eachItem) => {
			eachItem.gameObject.SetActive(false);
		});
	}

	void InitializeSummons() {
		int ctr = 0;
		foreach (KeyValuePair<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges) {
			SummonSettings settings = CharacterManager.Instance.GetSummonSettings(entry.Key);
			CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
			CharacterClassData cData = CharacterManager.Instance.GetOrCreateCharacterClassData(cClass.className);
			if (ctr < m_summonList.Count) {
				m_summonList[ctr].gameObject.SetActive(true);
				m_summonList[ctr++].InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges, cData.combatBehaviourType == CHARACTER_COMBAT_BEHAVIOUR.Tower);
			} else {
				AvailableMonsterItemUI summonItem = Instantiate(m_availableMonsterItemUI);
				summonItem.InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges, entry.Value.maxCharges, cData.combatBehaviourType == CHARACTER_COMBAT_BEHAVIOUR.Tower);
				summonItem.transform.SetParent(m_maraudUIView.GetAvailableSummonsParent());
				m_summonList.Add(summonItem);
				m_summonList[ctr++].onClicked += OnAvailableMonsterClicked;
			}
		}
		m_maraudUIView.ProcessSummonDisplay();
	}

	void InitializeTargets() {
		int ctr = 0;
		m_targetPartyStructure.allPossibleTargets.ForEach((EachTarget) => {
			if (ctr < m_targetList.Count) {
				m_targetList[ctr].gameObject.SetActive(true);
				m_targetList[ctr++].InitializeItem(EachTarget);
			} else {
				AvailableTargetItemUI availableTargetItemUI = Instantiate(m_availableTargetItemUI);
				availableTargetItemUI.InitializeItem(EachTarget);
				availableTargetItemUI.transform.SetParent(m_maraudUIView.GetAvailableTargetParent());
				m_targetList.Add(availableTargetItemUI);
				m_targetList[ctr++].onClicked += OnAvailableTargetClicked;
			}
		});
	}

	void InitializeMinions() {
		int ctr = 0;
		foreach (PLAYER_SKILL_TYPE eachSkill in PlayerSkillManager.Instance.allMinionPlayerSkills) {
			SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill);
			if (ctr < m_minionList.Count) {
				MinionSettings settings = CharacterManager.Instance.GetMinionSettings((skillData as MinionPlayerSkill).minionType);
				CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
				m_minionList[ctr].gameObject.SetActive(true);
				m_minionList[ctr++].InitializeItem(cClass, eachSkill, manaCostToDeploySummon, skillData.charges, skillData.baseMaxCharges);

			} else {
				MinionSettings settings = CharacterManager.Instance.GetMinionSettings((skillData as MinionPlayerSkill).minionType);
				CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
				AvailableMonsterItemUI minionItem = Instantiate(m_availableMonsterItemUI);
				minionItem.InitializeItem(cClass, eachSkill, manaCostToDeploySummon, skillData.charges, skillData.baseMaxCharges);
				minionItem.transform.SetParent(m_maraudUIView.GetAvailableMinionsParent());
				m_minionList.Add(minionItem);
				m_minionList[ctr++].onClicked += OnAvailableMonsterClicked;
			}
		}
	}
	void OnAvailableTargetClicked(AvailableTargetItemUI p_itemUI) {
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

	void OnAvailableMonsterClicked(AvailableMonsterItemUI p_clickedItem) {
		if (m_isTeamDeployed) {
			return;
		}
		if (!p_clickedItem.isMinion && m_targetPartyStructure.partyData.readyForDeploySummonCount < m_targetPartyStructure.partyData.maxSummonLimitDeployCount) {
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
			m_targetPartyStructure.partyData.readyForDeploySummonCount++;
			for (int x = 0; x < m_deployedSummonsUI.Count; ++x) {
				if (!m_deployedSummonsUI[x].isReadyForDeploy && !m_deployedSummonsUI[x].isDeployed) {
					m_deployedSummonsUI[x].gameObject.SetActive(true);
					m_deployedSummonsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
					break;
				}
			}
			m_maraudUIView.ProcessSummonDisplay();
		}
		if (p_clickedItem.isMinion && m_targetPartyStructure.partyData.deployedMinionCount <= 0) {
			bool exitLoop = false;
			for (int x = 0; x < m_deployedMinionsUI.Count && !exitLoop; ++x) {
				if (m_deployedMinionsUI[x].isReadyForDeploy) {
					for (int y = 0; y < m_minionList.Count; ++y) {
						if (m_minionList[y].playerSkillType == m_deployedMinionsUI[x].playerSkillType) {
							m_minionList[y].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
							p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
							m_deployedMinionsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.playerSkillType);
							exitLoop = true;
							m_maraudUIView.HideMinionButtonShowMinionContainer();
							m_targetPartyStructure.partyData.readyForDeployMinionCount++;
							break;
						}
					}
				} else if (!m_deployedMinionsUI[x].isDeployed && !m_deployedMinionsUI[x].isReadyForDeploy) {
					p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
					m_targetPartyStructure.partyData.readyForDeployMinionCount++;
					m_deployedMinionsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.playerSkillType);
					m_maraudUIView.HideMinionButtonShowMinionContainer();
					break;
				}
			}
		}
		ProcessButtonAvailability();
	}

	void ProcessButtonAvailability() {
		if (m_targetPartyStructure.IsAvailableForTargeting()) {
			m_maraudUIView.DisableDeployButton();
			return;
		} 
		if (!m_isTeamDeployed) {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0 && m_targetPartyStructure.partyData.readyForDeployTargetCount > 0) {
				m_maraudUIView.EnableDeployButton();
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
			for (int x = 0; x < m_summonList.Count; ++x) {
				if (m_summonList[x].characterClass == p_itemUI.characterClass && (p_itemUI.isDeployed || p_itemUI.isReadyForDeploy)) {
					m_targetPartyStructure.partyData.readyForDeploySummonCount--;
					m_targetPartyStructure.RemoveItemOnRight(p_itemUI);
					if (p_itemUI.isDeployed) {
						PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(m_summonList[x].summonType, 1);
						m_targetPartyStructure.RemoveCharacterOnList(p_itemUI.deployedCharacter);
						p_itemUI.UndeployCharacter();
					}
					p_itemUI.ResetButton();
					m_summonList[x].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
					p_itemUI.gameObject.SetActive(false);
				}
				m_maraudUIView.ProcessSummonDisplay();
			}
		} else {
			for (int x = 0; x < m_minionList.Count; ++x) {
				if (m_minionList[x].playerSkillType == p_itemUI.playerSkillType && (p_itemUI.isReadyForDeploy)) {
					m_minionList[x].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
					m_maraudUIView.ShowMinionButtonHideMinionContainer();
					m_targetPartyStructure.partyData.readyForDeployMinionCount--;
				}
			}
		}
		ProcessButtonAvailability();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetPartyStructure.partyData.maxSummonLimitDeployCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if ((m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0 || m_targetPartyStructure.partyData.readyForDeployTargetCount <= 0) && !m_isTeamDeployed) {
			return; //TODO: MESSAGE PLAYER THAT HE NEEDS LEADER
		}
		if (m_isTeamDeployed) { //this if is the UNDEPLOY trigger
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
			return; // <----- we have a return here 
		}
		LocationStructure structureToBePlaced = m_targetPartyStructure;
		if (!structureToBePlaced.structureType.IsOpenSpace()) {
			structureToBePlaced = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
		}
		m_deployedSummonsUI.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				Summon summon = CharacterManager.Instance.CreateNewSummon(eachMonsterToBeDeployed.summonType, PlayerManager.Instance.player.playerFaction, m_targetPartyStructure.currentSettlement, bypassIdeologyChecking: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, structureToBePlaced.GetRandomPassableTile());
                summon.OnSummonAsPlayerMonster();
                summon.SetDeployedAtStructure(m_targetPartyStructure);
				eachMonsterToBeDeployed.Deploy(summon);
				m_targetPartyStructure.AddDeployedItem(eachMonsterToBeDeployed);
				PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(eachMonsterToBeDeployed.summonType, -1);
			}
		});
		if (m_deployedMinionsUI[0].isReadyForDeploy && m_deployedMinionsUI[0].isMinion) {
			SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(m_deployedMinionsUI[0].playerSkillType);
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
	}

	public void OnCloseClicked() {
		HideAvailableItems();
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
		if(m_targetPartyStructure.IsAvailableForTargeting()) {
			Tooltip.Instance.ShowSmallInfo("Can't build team, structure is occupied.", "Occupied Structure", autoReplaceText: false);
			return;
		}
		if (m_isTeamDeployed) {
			Tooltip.Instance.ShowSmallInfo("Disband the team.", "Undeploy team", autoReplaceText: false);
		} else {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0 && m_targetPartyStructure.partyData.readyForDeployTargetCount > 0) {
				Tooltip.Instance.ShowSmallInfo("Send the team to do the quest.", "Deploy team", autoReplaceText: false);
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