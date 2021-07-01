using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Ruinarch;

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
	private int m_totalDeployCost = 0;

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
			eachDeployedItem.onDelete += OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onUnlockClicked += OnUnlockSlotClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onAddSummonClicked += OnAddSummonClicked;
		});
	}

	void UnlistenToDeployedItems() {
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete -= OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onUnlockClicked -= OnUnlockSlotClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onAddSummonClicked -= OnAddSummonClicked;
		});
	}
	private void OnDefensePointClicked(LocationStructure p_clickedDefensePoint) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetPartyStructure = p_clickedDefensePoint as PartyStructure;
			Init();
		}
	}
	public void Init() {
		m_totalDeployCost = 0;
		m_targetPartyStructure.InitializeTeam();
		ReturnAllItemToPool();
		InstantiateUI();
		InitializeSummons();
		InitializeDeployedItems();
		m_defensePointUIView.SetTitle("Prism");
		ProcessDeployButtonDisplay();
		InputManager.Instance.SetAllHotkeysEnabledState(false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
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
		m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		m_targetPartyStructure.partyData.ResetAllReadyCounts();
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetPartyStructure.partyData.deployedSummonUnderlings.Count; ++x) {
			m_deployedSummonsUI[x].gameObject.SetActive(true);
			m_deployedSummonsUI[x].HideManaCost();
			m_deployedSummonsUI[x].InitializeItem(m_targetPartyStructure.partyData.deployedSummonUnderlings[x], true, false);
			m_deployedSummonsUI[x].Deploy(m_targetPartyStructure.partyData.deployedSummons[x], true);
		}
		m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
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
            if (entry.Value.hasMaxCharge) {
				GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_defensePointUIView.GetAvailableSummonsParent());
				MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
				item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
				item.SetObject(entry.Value);
				item.SetAsButton();
				item.SetInteractableState(entry.Value.currentCharges > 0);
				m_summonList.Add(item);
				item.AddHoverEnterAction(OnHoverItemOccupiedStructure);
				item.AddHoverExitAction(OnHoverExitItemOccupiedStructure);
			}
		}
		m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
	}

	void OnAvailableMonsterClicked(MonsterAndDemonUnderlingCharges p_clickedMonster, MonsterUnderlingQuantityNameplateItem p_item) {
		if (m_targetPartyStructure.partyData.readyForDeploySummonCount + m_targetPartyStructure.partyData.deployedSummonCount >= m_targetPartyStructure.startingSummonCount) {
			return;
		}
		
		if (!p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeploySummonCount < m_targetPartyStructure.partyData.maxSummonLimitDeployCount) {
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedSummonsUI, p_clickedMonster);
			m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		}
		ProcessDeployButtonDisplay();
	}

	void ProcessDeployedItemFromClickingAvailableItem(List<DeployedMonsterItemUI> deployedItemList, MonsterAndDemonUnderlingCharges p_monsterClicked) {
		for (int x = 0; x < deployedItemList.Count; ++x) {
			if (!deployedItemList[x].isReadyForDeploy && !deployedItemList[x].isDeployed) {
				deployedItemList[x].gameObject.SetActive(true);
				deployedItemList[x].InitializeItem(p_monsterClicked);
				m_totalDeployCost += deployedItemList[x].summonCost;
				deployedItemList[x].ShowManaCost();
				deployedItemList[x].ShowRemoveButton();
				m_targetPartyStructure.partyData.readyForDeploySummonCount++;
				break;
			}
		}
	}

	void ProcessAvailableItemFromClickingDeployedItem(List<MonsterUnderlingQuantityNameplateItem> availItems, DeployedMonsterItemUI p_itemUI) {
		availItems.ForEach((availableSummons) => {
			if (availableSummons.obj.characterClassName == p_itemUI.obj.characterClassName) {
				availableSummons.IncreaseOneChargeForDisplayPurpose();
				if (p_itemUI.isDeployed) {
					m_targetPartyStructure.RemoveCharacterOnList(p_itemUI.deployedCharacter);
				} else {
					m_totalDeployCost -= p_itemUI.summonCost;
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
			m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		} 
		ProcessDeployButtonDisplay();
	}

	void OnUnlockSlotClicked(DeployedMonsterItemUI p_itemUI) {
		if (m_targetPartyStructure.startingSummonCount < m_targetPartyStructure.MAX_SUMMON_COUNT && PlayerManager.Instance.player.plagueComponent.plaguePoints >= p_itemUI.GetUnlockCost()) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-p_itemUI.GetUnlockCost());
			m_targetPartyStructure.startingSummonCount++;
			m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		}
	}

	public void OnAddSummonClicked() { m_defensePointUIView.ShowSummonSubContainer(); m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints); }

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if (!m_isAllItemDeployed) {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Deploy Party.", $"Are you sure you want to spend {m_totalDeployCost.ToString()}{UtilityScripts.Utilities.ManaIcon()} to summon this party?", OnYesDeploy, showCover: true, layer: 150);
		} else {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Disband Party.", $"Are you sure you want to disband the party?", OnYesUndeploy, showCover: true, layer: 150);
		}
	}

	void OnYesDeploy() {
		int newDeployedCount = 0;
		m_deployedSummonsUI.ForEach((eachSummonToBeDeployed) => {
			if (eachSummonToBeDeployed.isReadyForDeploy) {
				Summon summon = CharacterManager.Instance.CreateNewSummon(eachSummonToBeDeployed.obj.monsterType, PlayerManager.Instance.player.playerFaction, PlayerManager.Instance.player.playerSettlement);
				CharacterManager.Instance.PlaceSummonInitially(summon, m_targetPartyStructure.GetRandomPassableTile());
				summon.OnSummonAsPlayerMonster();
				eachSummonToBeDeployed.HideManaCost();
				eachSummonToBeDeployed.Deploy(summon, true);
				m_targetPartyStructure.AddDeployedItem(eachSummonToBeDeployed);
				PlayerManager.Instance.player.underlingsComponent.DecreaseMonsterUnderlingCharge(eachSummonToBeDeployed.obj.monsterType);
				newDeployedCount++;
			}
		});
		if (newDeployedCount > 0) {
			m_targetPartyStructure.DeployParty();
		}
		ProcessDeployButtonDisplay();
		PlayerManager.Instance.player.AdjustMana(-m_totalDeployCost);
		m_totalDeployCost = 0;
		OnCloseClicked();
	}

	void OnYesUndeploy() {
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
		m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		ProcessDeployButtonDisplay();
	}

	int displayedCount = 0;
	private void ProcessDeployButtonDisplay() {
		displayedCount = 0;
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

		m_defensePointUIView.EnableDeployButton();
		if (!m_isAllItemDeployed) {
			if (displayedCount <= 0) {
				m_defensePointUIView.DisableDeployButton();
				return;
			}

			if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
				m_defensePointUIView.DisableDeployButton();
				return;
			}
		}
	}

	public void OnCloseClicked() {
		HideSummonItems();
		ReturnAllItemToPool();
		HideUI();
		m_defensePointUIView.HideAllSubMenu();
		InputManager.Instance.SetAllHotkeysEnabledState(true);
		UIManager.Instance.ResumeLastProgressionSpeed();
	}

	void OnHoverItemOccupiedStructure(MonsterAndDemonUnderlingCharges p_monsterAndDemonUnderlingCharges) {
		CharacterClassData data = CharacterManager.Instance.GetOrCreateCharacterClassData(p_monsterAndDemonUnderlingCharges.characterClassName);
		if (data.combatBehaviourType != CHARACTER_COMBAT_BEHAVIOUR.None) {
			CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(data.combatBehaviourType);
			UIManager.Instance.ShowSmallInfo(combatBehaviour.description, m_defensePointUIView.UIModel.hoverPosition, combatBehaviour.name);
		}
		/*
		if (p_monsterAndDemonUnderlingCharges.isDemon) {
			MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_monsterAndDemonUnderlingCharges.minionType);
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(minionPlayerSkill, PlayerUI.Instance.minionListHoverPosition);
		}*/
		if (!p_monsterAndDemonUnderlingCharges.isDemon && p_monsterAndDemonUnderlingCharges.isReplenishing) {
			PlayerUI.Instance.OnHoverSpellChargeRemainingForSummon(data, p_monsterAndDemonUnderlingCharges);
		} else if (p_monsterAndDemonUnderlingCharges.isDemon && p_monsterAndDemonUnderlingCharges.isReplenishing) {
			SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_monsterAndDemonUnderlingCharges.minionType);
			PlayerUI.Instance.OnHoverSpellChargeRemaining(skillData, p_monsterAndDemonUnderlingCharges);
		}
	}

	void OnHoverExitItemOccupiedStructure(MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges) {
		UIManager.Instance.HideSmallInfo();
	}

	public void OnCloseSummonSubContainer() { m_defensePointUIView.HideAllSubMenu(); m_defensePointUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, PlayerManager.Instance.player.plagueComponent.plaguePoints); }

	public void OnHoverOver() {
		if (m_isAllItemDeployed) {
			Tooltip.Instance.ShowSmallInfo("Unsummon all party members.", "Undeploy Party", autoReplaceText: false);
		} else {
			if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
				Tooltip.Instance.ShowSmallInfo("Can't build team, Not enough Mana.", "Not enough Mana", autoReplaceText: false);
				return;
			}
			if (displayedCount > 0) {
				Tooltip.Instance.ShowSmallInfo("Summon minions to defend around the Prism.", "Deploy team", autoReplaceText: false);
				return;
			}
		}
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
	}
	#endregion
}