using Inner_Maps.Location_Structures;
using Ruinarch.MVCFramework;
using System.Collections.Generic;
using UnityEngine;
using Ruinarch;
using Inner_Maps;
using Tutorial;
using UtilityScripts;

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

	private LocationGridTile m_chosenTile;

	private void Start() {
		UIManager.Instance.onMaraudClicked += OnMaraudClicked;
		UIManager.Instance.onKennelClicked += OnKennelClicked;
		UIManager.Instance.onTortureChamberClicked += OnTortureChambersClicked;
		Messenger.AddListener<LocationGridTile>(PartySignals.PARTY_TILE_CHOSEN_FOR_SPAWNING, OnTileChosenForSpawning);
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
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onUnlockClicked += OnUnlockSlotClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onAddSummonClicked += OnAddSummonClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDeleteClick += OnDeployedTargetClicked;
			eachDeployedItem.onHoverOver += OnHoverOverDeployedItem;
			eachDeployedItem.onHoverOut += OnHoverOutDeployedItem;
		});
	}
	void UnlistenToDeployedItems() {
		m_deployedMinionsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete -= OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDelete -= OnDeployedMonsterClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onUnlockClicked -= OnUnlockSlotClicked;
		});
		m_deployedSummonsUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onAddSummonClicked -= OnAddSummonClicked;
		});
		m_deployedTargetItemUI.ForEach((eachDeployedItem) => {
			eachDeployedItem.onDeleteClick -= OnDeployedTargetClicked;
			eachDeployedItem.onHoverOver -= OnHoverOverDeployedItem;
			eachDeployedItem.onHoverOut -= OnHoverOutDeployedItem;
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
		UpdateNoTargetsUI();
		m_maraudUIView.SetTitle(p_title);
		if (m_isTeamDeployed) {
			m_maraudUIView.SetButtonDeployText("Undeploy");
		} else {
			m_maraudUIView.SetButtonDeployText("Deploy");
		}
		if (p_title != string.Empty) {
			m_maraudUIView.SetTitle(p_title);
		}
		
		ProcessButtonAvailability();
		InputManager.Instance.SetAllHotkeysEnabledState(false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
		UIManager.Instance.Pause();

	}

	#region deployed items
	void InitializeDeployedItems() {
		HideDeployedItems();
		DisplayDeployedItems();
		DisplayDeployedDeadMembers();
		m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
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
			m_deployedSummonsUI[x].Deploy(m_targetPartyStructure.partyData.deployedSummons[x]);
		}
		if (m_targetPartyStructure.partyData.deployedMinionCount > 0) {
			m_maraudUIView.HideMinionButtonShowMinionContainer();
			m_deployedMinionsUI[0].gameObject.SetActive(true);
			m_deployedMinionsUI[0].Deploy(m_targetPartyStructure.partyData.deployedMinions[0]);
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

	void DisplayDeployedDeadMembers() {
		if (m_targetPartyStructure.party != null) {
			m_targetPartyStructure.party.deadMembers.ForEach((eachDeadMembers) => Debug.Log(eachDeadMembers.nameWithID));
			m_targetPartyStructure.party.deadMembers.ForEach((eachMember) => {
				if (eachMember.minion != null) {
					m_deployedMinionsUI[0].gameObject.SetActive(true);
					MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(eachMember.minion.minionPlayerSkillType);
					m_deployedMinionsUI[0].InitializeItem(PlayerManager.Instance.player.underlingsComponent.GetMinionUnderlingChargesByMinionType(minionPlayerSkill.minionType));
					m_deployedMinionsUI[0].ShowDeadIcon();
					m_deployedMinionsUI[0].HideRemoveButton();
					m_maraudUIView.HideMinionButtonShowMinionContainer();
				} else {
					for(int x = 0; x < m_deployedSummonsUI.Count; ++x) {
						if (!m_deployedSummonsUI[x].isDeployed) {
							m_deployedSummonsUI[x].gameObject.SetActive(true);
							m_deployedSummonsUI[x].InitializeItem(PlayerManager.Instance.player.underlingsComponent.GetSummonUnderlingChargesBySummonType((eachMember as Summon).summonType));
							m_deployedSummonsUI[x].ShowDeadIcon();
							m_deployedSummonsUI[x].HideRemoveButton();
							break;
						}
					}
				}
			});
		}
	}
	#endregion

	#region Summons
	void InitializeSummons() {
		foreach (KeyValuePair<SUMMON_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges) {
			if (entry.Value.hasMaxCharge) {
				GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_maraudUIView.GetAvailableSummonsParent());
				MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
				item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
				item.SetObject(entry.Value);
				item.SetAsButton();
				m_summonList.Add(item);
				item.SetInteractableState(CharacterManager.Instance.GetOrCreateCharacterClassData(entry.Value.characterClassName).combatBehaviourType != CHARACTER_COMBAT_BEHAVIOUR.Tower
					&& entry.Value.currentCharges > 0);
				item.AddHoverEnterAction(OnHoverItemOccupiedStructure);
				item.AddHoverExitAction(OnHoverExitItemOccupiedStructure);
			}
		}
	}
	#endregion

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

	void OnHoverItemOccupiedStructure(MonsterAndDemonUnderlingCharges p_monsterAndDemonUnderlingCharges) {
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			UIManager.Instance.ShowSmallInfo("You cant add a team member bacause the structure is occupied", "Structure Occupied", true);
		} else {
			CharacterClassData data = CharacterManager.Instance.GetOrCreateCharacterClassData(p_monsterAndDemonUnderlingCharges.characterClassName);
			if (data.combatBehaviourType != CHARACTER_COMBAT_BEHAVIOUR.None) {
				CharacterCombatBehaviour combatBehaviour = CombatManager.Instance.GetCombatBehaviour(data.combatBehaviourType);
				UIManager.Instance.ShowSmallInfo(combatBehaviour.description, m_maraudUIView.UIModel.hoverPosition, combatBehaviour.name);
			}
			if (!p_monsterAndDemonUnderlingCharges.isDemon && p_monsterAndDemonUnderlingCharges.isReplenishing) {
				PlayerUI.Instance.OnHoverSpellChargeRemainingForSummon(data, p_monsterAndDemonUnderlingCharges);
			} else if(p_monsterAndDemonUnderlingCharges.isDemon && p_monsterAndDemonUnderlingCharges.isReplenishing){
				SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_monsterAndDemonUnderlingCharges.minionType); 
				PlayerUI.Instance.OnHoverSpellChargeRemaining(skillData, p_monsterAndDemonUnderlingCharges);
			}
			/*
			if (p_monsterAndDemonUnderlingCharges.isDemon) {
				MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(p_monsterAndDemonUnderlingCharges.minionType);
				PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(minionPlayerSkill, PlayerUI.Instance.minionListHoverPosition);
			}*/
		}
	}

	void OnHoverExitItemOccupiedStructure(MonsterAndDemonUnderlingCharges monsterAndDemonUnderlingCharges) {
		UIManager.Instance.HideSmallInfo();
	}

	void InitializeTargets() {
		int ctr = 0;
		m_targetPartyStructure.allPossibleTargets.ForEach((EachTarget) => {
			bool isValidTarget = IsValidTargetForStructure(m_targetPartyStructure, EachTarget);
			if (ctr < m_targetList.Count) {
				m_targetList[ctr].gameObject.SetActive(true);
				m_targetList[ctr].InitializeItem(EachTarget, !isValidTarget);
				SetTargetHoverText(m_targetList[ctr++]);
			} else {
				AvailableTargetItemUI availableTargetItemUI = Instantiate(m_availableTargetItemUI);
				availableTargetItemUI.InitializeItem(EachTarget, !isValidTarget);
				availableTargetItemUI.transform.SetParent(m_maraudUIView.GetAvailableTargetParent());
				availableTargetItemUI.transform.localScale = Vector3.one;
				m_targetList.Add(availableTargetItemUI);
				SetTargetHoverText(m_targetList[ctr]);
				AvailableTargetItemUI targetItemUI = m_targetList[ctr++];
				targetItemUI.onClicked += OnAvailableTargetClicked;
				targetItemUI.onHoverOver += OnHoverOverAvailableTargetItem;
				targetItemUI.onHoverOut += OnHoverOutAvailableTargetItem;
			}
		});
	}

	private bool IsValidTargetForStructure(PartyStructure p_structure, IStoredTarget p_target) {
		if (p_target is Character targetCharacter) {
			if (p_target.isTargetted || (targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.KENNEL || targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
				return false;
			} else if (p_structure.structureType == STRUCTURE_TYPE.KENNEL && targetCharacter.traitContainer.HasTrait("Sturdy")) {
				//Sturdy characters cannot be targeted by snatch
				//Reference: https://trello.com/c/PpwfezCb/4679-live-v040-harpy-trying-to-abduct-a-dragon
				return false;
			} else {
				return true;
			}
		} else {
			return true;
		}
	}
	void UpdateNoTargetsUI() {
		bool hasAvailableTargets = m_targetPartyStructure.allPossibleTargets.Count > 0;
		if (hasAvailableTargets) {
			m_maraudUIView.HideNoTargetsUI();
		}
		else {
			string text = string.Empty;
			if (m_targetPartyStructure is Kennel) {
				text = "Your Targets List does not have any valid Snatch target. Store the monster you intend to snatch on your Targets List.";
			} else if (m_targetPartyStructure is TortureChambers) {
				text = "Your Targets List does not have any valid Snatch target. Store the Villager you intend to snatch on your Targets List.";
			}
			if (!string.IsNullOrEmpty(text)) {
				m_maraudUIView.ShowNoTargetsUI(text);	
			}
		}
	}

	void SetTargetHoverText(AvailableTargetItemUI p_item) {
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			p_item.SetHoverText("You cant add a team member because the structure is occupied");
		} else if (p_item.target is Character character) {
			if (character.traitContainer.HasTrait("Sturdy") && m_targetPartyStructure.structureType == STRUCTURE_TYPE.KENNEL) {
				p_item.SetHoverText("Cannot target Sturdy characters");
			} else if(character.currentStructure.structureType == STRUCTURE_TYPE.KENNEL ||
			     character.currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS) {
				p_item.SetHoverText("Target already imprisoned");
			}
		} else {
			p_item.SetHoverText("Target already chased by another party");
		}
	}

	void InitializeMinions() {
		foreach (KeyValuePair<MINION_TYPE, MonsterAndDemonUnderlingCharges> entry in PlayerManager.Instance.player.underlingsComponent.demonUnderlingCharges) {
			SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(entry.Value.minionType);
			if (skillData.isInUse || skillData.isTemporarilyInUse) {
				MinionSettings settings = CharacterManager.Instance.GetMinionSettings(entry.Value.minionType);
				CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
				GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(m_availableMonsterItemUI.name, Vector3.zero, Quaternion.identity, m_maraudUIView.GetAvailableMinionsParent());
				MonsterUnderlingQuantityNameplateItem item = go.GetComponent<MonsterUnderlingQuantityNameplateItem>();
				item.AddOnClickAction((monsterCharge) => { OnAvailableMonsterClicked(monsterCharge, item); });
				item.SetObject(entry.Value);
				item.SetAsButton();
				m_minionList.Add(item);
				bool canDoAbility = skillData.CanPerformAbility();
				item.SetInteractableState(canDoAbility);
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
		UIManager.Instance.HideCharacterNameplateTooltip();
		UIManager.Instance.HideStructureNameplateTooltip();
		ProcessButtonAvailability();
		m_maraudUIView.HideAllSubMenu();
	}
	void OnHoverOverAvailableTargetItem(AvailableTargetItemUI p_target) {
		if (p_target.target is Character character) {
			UIManager.Instance.ShowCharacterNameplateTooltip(character, m_maraudUIView.UIModel.hoverPosition);
		} else if (p_target.target is TileObject tileObject) {
			UIManager.Instance.ShowTileObjectNameplateTooltip(tileObject, m_maraudUIView.UIModel.hoverPosition);
		} else if (p_target.target is LocationStructure structure) {
			UIManager.Instance.ShowStructureNameplateTooltip(structure, m_maraudUIView.UIModel.hoverPosition);
		}
	}
	void OnHoverOutAvailableTargetItem(AvailableTargetItemUI p_target) {
		if (p_target.target is Character) {
			UIManager.Instance.HideCharacterNameplateTooltip();
		} else if (p_target.target is TileObject) {
			UIManager.Instance.HideTileObjectNameplateTooltip();
		} else if (p_target.target is LocationStructure) {
			UIManager.Instance.HideStructureNameplateTooltip();
		} else {
			UIManager.Instance.HideCharacterNameplateTooltip();
			UIManager.Instance.HideTileObjectNameplateTooltip();
			UIManager.Instance.HideStructureNameplateTooltip();
		}
	}

	void OnAvailableMonsterClicked(MonsterAndDemonUnderlingCharges p_clickedMonster, MonsterUnderlingQuantityNameplateItem p_item) {
		if (m_isTeamDeployed) {
			return;
		}
		if (!m_targetPartyStructure.IsAvailableForTargeting()) {
			return;
		}
		
		if (!p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeploySummonCount < m_targetPartyStructure.partyData.maxSummonLimitDeployCount) {
			if (m_targetPartyStructure.partyData.readyForDeploySummonCount >= m_targetPartyStructure.startingSummonCount) {
				return;
			}
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedSummonsUI, p_clickedMonster);
			m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
			if (m_targetPartyStructure.partyData.readyForDeploySummonCount + m_targetPartyStructure.partyData.deployedSummonCount >= m_targetPartyStructure.startingSummonCount) {
				m_maraudUIView.HideAllSubMenu();
			}
		} else if (p_item.obj.isDemon && m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0) {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount >= 1) {
				return;
			}
			p_item.DeductOneChargeForDisplayPurpose();
			ProcessDeployedItemFromClickingAvailableItem(m_deployedMinionsUI, p_clickedMonster);
			m_maraudUIView.HideMinionButtonShowMinionContainer();
			m_maraudUIView.HideAllSubMenu();
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
			if (availableSummons.obj.characterClassName == p_itemUI.obj.characterClassName) {
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
			} else {
				m_maraudUIView.DisableDeployButton();
			}

			if (!m_targetPartyStructure.IsAvailableForTargeting()) {
				m_maraudUIView.DisableDeployButton();
			}

			if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
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
		OnHoverOutDeployedItem(p_itemUI);
	}
	private void OnHoverOutDeployedItem(DeployedTargetItemUI p_target) {
		if (p_target.target is Character) {
			UIManager.Instance.HideCharacterNameplateTooltip();
		} else if (p_target.target is TileObject) {
			UIManager.Instance.HideTileObjectNameplateTooltip();
		} else if (p_target.target is LocationStructure) {
			UIManager.Instance.HideStructureNameplateTooltip();
		} else {
			UIManager.Instance.HideCharacterNameplateTooltip();
			UIManager.Instance.HideTileObjectNameplateTooltip();
			UIManager.Instance.HideStructureNameplateTooltip();
		}
	}
	private void OnHoverOverDeployedItem(DeployedTargetItemUI p_target) {
		if (p_target.target is Character character) {
			UIManager.Instance.ShowCharacterNameplateTooltip(character, m_maraudUIView.UIModel.deployedItemHoverPosition);
		} else if (p_target.target is TileObject tileObject) {
			UIManager.Instance.ShowTileObjectNameplateTooltip(tileObject, m_maraudUIView.UIModel.deployedItemHoverPosition);
		} else if (p_target.target is LocationStructure structure) {
			UIManager.Instance.ShowStructureNameplateTooltip(structure, m_maraudUIView.UIModel.deployedItemHoverPosition);
		}
	}

	void OnUnlockSlotClicked(DeployedMonsterItemUI p_itemUI) {
		if (m_targetPartyStructure.startingSummonCount < m_targetPartyStructure.MAX_SUMMON_COUNT && PlayerManager.Instance.player.plagueComponent.plaguePoints >= p_itemUI.GetUnlockCost()) {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-p_itemUI.GetUnlockCost());
			m_targetPartyStructure.startingSummonCount++;
			m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		}
	}

	public void OnAddSummonClicked() { m_maraudUIView.ShowSummonSubContainer(); ProcessDeployedItemsDisplay(); }

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		if (m_isTeamDeployed) {
			return;
		}
		if (!p_itemUI.isMinion) {
			ProcessAvailableItemFromClickingDeployedItem(m_summonList, p_itemUI);
			m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		} else {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0) {
				ProcessAvailableItemFromClickingDeployedItem(m_minionList, p_itemUI);
				m_maraudUIView.ShowMinionButtonHideMinionContainer();
			}
		}
		ProcessButtonAvailability();
	}

	void OnTileChosenForSpawning(LocationGridTile p_chosenTile) {
		if ((m_targetPartyStructure.partyData.readyForDeployMinionCount <= 0 || m_targetPartyStructure.partyData.readyForDeployTargetCount <= 0) && !m_isTeamDeployed) {
			return; //TODO: MESSAGE PLAYER THAT HE NEEDS LEADER
		}
		m_chosenTile = p_chosenTile;
		if (!m_isTeamDeployed) {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Deploy Party.", $"Are you sure you want to spend {m_totalDeployCost.ToString()}{UtilityScripts.Utilities.ManaIcon()} to summon this party?", OnYesDeploy, showCover: true, layer: 150);
		}
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if (m_isTeamDeployed) {
			UIManager.Instance.yesNoConfirmation.ShowYesNoConfirmation("Disband Party.", $"Are you sure you want to disband the party?", OnYesUndeploy, showCover: true, layer: 150);
		} else {
			(PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.SPAWN_PARTY) as SpawnPartyData).Activate();
			HideUI();
		} 
	}
	void OnYesDeploy() {
		if (m_deployedMinionsUI[0].isReadyForDeploy && m_deployedMinionsUI[0].isMinion) {
			SkillData skillData = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(m_deployedMinionsUI[0].obj.minionType);
			Character minion = null;
			skillData.ActivateAbility(m_chosenTile.GetRandomNeighborWithoutCharacters(), ref minion);
			minion.SetDeployedAtStructure(m_targetPartyStructure);
			m_deployedMinionsUI[0].Deploy(minion);
			m_targetPartyStructure.AddDeployedItem(m_deployedMinionsUI[0]);
		}
		m_deployedSummonsUI.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				Summon summon = CharacterManager.Instance.CreateNewSummon(eachMonsterToBeDeployed.obj.monsterType, PlayerManager.Instance.player.playerFaction, PlayerManager.Instance.player.playerSettlement, bypassIdeologyChecking: true);
				CharacterManager.Instance.PlaceSummonInitially(summon, m_chosenTile.GetRandomNeighborWithoutCharacters());
				summon.OnSummonAsPlayerMonster();
				summon.SetDeployedAtStructure(m_targetPartyStructure);
				eachMonsterToBeDeployed.Deploy(summon);
				m_targetPartyStructure.AddDeployedItem(eachMonsterToBeDeployed);
				PlayerManager.Instance.player.underlingsComponent.DecreaseMonsterUnderlingCharge(eachMonsterToBeDeployed.obj.monsterType);
			}
		});

		if (m_deployedTargetItemUI[0].isReadyForDeploy) {
			m_deployedTargetItemUI[0].Deploy();
			m_targetPartyStructure.AddTargetOnCurrentList(m_deployedTargetItemUI[0].target);
		}
		
		m_targetPartyStructure.DeployParty();
		m_isTeamDeployed = true;
		m_maraudUIView.SetButtonDeployText("Undeploy");
		PlayerManager.Instance.player.AdjustMana(-m_totalDeployCost);
		m_totalDeployCost = 0;
		OnCloseClicked();
		PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
	}

	void ProcessDeployedItemsDisplay() {
		m_maraudUIView.ProcessMinionDisplay(m_targetPartyStructure.partyData.readyForDeployMinionCount + m_targetPartyStructure.partyData.deployedMinionCount);
		m_maraudUIView.ProcessTargetDisplay(m_targetPartyStructure.partyData.readyForDeployTargetCount + m_targetPartyStructure.partyData.deployedTargetCount);
		m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
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
		m_maraudUIView.ProcessSummonDisplay(m_targetPartyStructure.startingSummonCount, m_targetPartyStructure.MAX_SUMMON_COUNT, m_targetPartyStructure.party, PlayerManager.Instance.player.plagueComponent.plaguePoints);
		Init();
	}

	public void OnCloseClicked() {
		HideAvailableItems();
		ReturnAllItemToPool();
		HideUI();
		m_maraudUIView.HideAllSubMenu();
		InputManager.Instance.SetAllHotkeysEnabledState(true);
		UIManager.Instance.ResumeLastProgressionSpeed();
	}
	public void OnAddMinionClicked() { m_maraudUIView.ShowMinionSubContainer(); ProcessDeployedItemsDisplay(); }
	public void OnAddTargetClicked() { m_maraudUIView.ShowTargetSubContainer(); ProcessDeployedItemsDisplay(); }

	public void OnCloseSummonSubContainer() { m_maraudUIView.HideAllSubMenu(); ProcessDeployedItemsDisplay(); }
	public void OnCloseMinionSubContainer() { m_maraudUIView.HideAllSubMenu(); ProcessDeployedItemsDisplay(); }
	public void OnCloseTargetSubContainer() { m_maraudUIView.HideAllSubMenu(); ProcessDeployedItemsDisplay(); }

	public void OnHoverOver() {
		if (m_isTeamDeployed) {
			Tooltip.Instance.ShowSmallInfo("Unsummon all party members.", "Undeploy Party", autoReplaceText: false);
		} else {
			if (m_targetPartyStructure.partyData.readyForDeployMinionCount > 0 && m_targetPartyStructure.partyData.readyForDeployTargetCount > 0) {
				Tooltip.Instance.ShowSmallInfo("Spawn the party to do the task", "Deploy Party", autoReplaceText: false);
			}  else {
				Tooltip.Instance.ShowSmallInfo("Should at least have a Target and a Leader", "Deploy team", autoReplaceText: false);
			}

			if (!m_targetPartyStructure.IsAvailableForTargeting()) {
				Tooltip.Instance.ShowSmallInfo("Can't build team, structure is occupied.", "Occupied Structure", autoReplaceText: false);
			}
			
			if (m_totalDeployCost > PlayerManager.Instance.player.mana) {
				Tooltip.Instance.ShowSmallInfo("Can't build team, Not enough Mana", "Not enough Mana", autoReplaceText: false);
			}
		}
	}

	public void OnHoverOut() {
		Tooltip.Instance.HideSmallInfo();
		UIManager.Instance.HideSmallInfo();
	}
	public void OnClickNoTargetsTip() {
		PlayerUI.Instance.ShowSpecificTutorial(TutorialManager.Tutorial_Type.Storing_Targets);
	}
	#endregion
}