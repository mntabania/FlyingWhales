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
		InitializeMinions();
		InitializeDeployedItems();
	}

	void HideDeployedItems() {
		int x = 0;
		for (; x < m_targetMaraudStructure.maxSummonLimitDeployCount; ++x) {
			m_deployedSummonsUI[x].MakeSlotEmpty();
		}
		for (; x < 4; ++x) {
			m_deployedSummonsUI[x].MakeSlotLocked();
		}
		m_deployedMinionsUI[0].MakeSlotEmpty();
		m_targetMaraudStructure.readyForDeployMinionCount = 0;
		m_targetMaraudStructure.readyForDeploySummonCount = 0;
	}

	void DisplayDeployedItems() {
		for (int x = 0; x < m_targetMaraudStructure.deployedCSummonlass.Count; ++x) {
			m_deployedSummonsUI[x].InitializeItem(m_targetMaraudStructure.deployedCSummonlass[x], m_targetMaraudStructure.deployedSummonSettings[x], m_targetMaraudStructure.deployedSummonType[x], true);
		}
		if (m_targetMaraudStructure.deployedMinionCount > 0) {
			m_deployedMinionsUI[0].InitializeItem(m_targetMaraudStructure.deployedMinionsSkillType[0], true);
		} else {
			m_deployedMinionsUI[0].MakeSlotEmpty();
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
		m_minionList.ForEach((eachItem) => {
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

	void InitializeMinions() {
		int ctr = 0;
		foreach (PLAYER_SKILL_TYPE eachSkill in PlayerSkillManager.Instance.allMinionPlayerSkills) {
			SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill);
			if (ctr < m_minionList.Count) {
				m_minionList[ctr].gameObject.SetActive(true);
				m_minionList[ctr].onClicked += OnAvailableMonsterClicked;
				m_minionList[ctr++].InitializeItem(eachSkill, manaCostToDeploySummon, skillData.charges, skillData.baseMaxCharges);
			} else {
				AvailableMonsterItemUI minionItem = Instantiate(m_availableMonsterItemUI);
				minionItem.InitializeItem(eachSkill, manaCostToDeploySummon, skillData.charges, skillData.baseMaxCharges);
				minionItem.transform.SetParent(m_maraudUIView.GetAvailableMinionsParent());
				m_minionList.Add(minionItem);
				m_minionList[ctr++].onClicked += OnAvailableMonsterClicked;
			}
		}
	}

	void OnAvailableMonsterClicked(AvailableMonsterItemUI p_clickedItem) {
		if (m_targetMaraudStructure.readyForDeploySummonCount + m_targetMaraudStructure.deployedSummonCount < m_targetMaraudStructure.maxSummonLimitDeployCount) {
			int price = manaCostToDeploySummon;
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < price);
			if (!p_clickedItem.isMinion) {
				m_targetMaraudStructure.readyForDeploySummonCount++;
				for (int x = 0; x < m_deployedSummonsUI.Count; ++x) {
					if (!m_deployedSummonsUI[x].isReadyForDeploy && !m_deployedSummonsUI[x].isDeployed) {
						m_deployedSummonsUI[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
						break;
					}
				}
			} else {
				bool exitLoop = false;
				for (int x = 0; x < m_deployedMinionsUI.Count && !exitLoop; ++x) {
					if (m_deployedMinionsUI[x].isReadyForDeploy || m_deployedMinionsUI[x].isDeployed) {
						for (int y = 0; y < m_minionList.Count; ++y) {
							if (m_minionList[y].playerSkillType == m_deployedMinionsUI[x].playerSkillType) {
								m_minionList[y].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
								m_deployedMinionsUI[x].InitializeItem(p_clickedItem.playerSkillType);
								exitLoop = true;
								break;
							}
						}
					} else if (!m_deployedMinionsUI[x].isDeployed && !m_deployedMinionsUI[x].isReadyForDeploy) {
						m_targetMaraudStructure.readyForDeployMinionCount++;
						m_deployedMinionsUI[x].InitializeItem(p_clickedItem.playerSkillType);
						break;
					}
				}
			}
		}
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) { //not just deployed, but also the one being planned out
		m_targetMaraudStructure.readyForDeploySummonCount--;
		m_targetMaraudStructure.RemoveItemOnRight(p_itemUI);
		if (!p_itemUI.isMinion) {
			for (int x = 0; x < m_summonList.Count; ++x) {
				if (m_summonList[x].characterClass == p_itemUI.characterClass && (p_itemUI.isDeployed || p_itemUI.isReadyForDeploy)) {
					if (p_itemUI.isDeployed) {
						PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[m_summonList[x].summonType].currentCharges++;	
					}
					m_summonList[x].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
				}
			}
		} else {
			for (int x = 0; x < m_minionList.Count; ++x) {
				if (m_minionList[x].playerSkillType == p_itemUI.playerSkillType && (p_itemUI.isReadyForDeploy)) {
					m_minionList[x].AddOneCharge(PlayerManager.Instance.player.mana < manaCostToDeploySummon);
				}
			}
		}
		p_itemUI.MakeSlotEmpty();
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		m_targetMaraudStructure.maxSummonLimitDeployCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		if (m_targetMaraudStructure.readyForDeployMinionCount <= 0) {
			return; //TODO: MESSAGE PLAYER THAT HE NEEDS LEADER
		}
		m_deployedSummonsUI.ForEach((eachMonsterToBeDeployed) => {
			if (eachMonsterToBeDeployed.isReadyForDeploy) {
				m_targetMaraudStructure.AddDeployedItem(eachMonsterToBeDeployed);
				eachMonsterToBeDeployed.Deploy();
				PlayerManager.Instance.player.underlingsComponent.monsterUnderlingCharges[eachMonsterToBeDeployed.summonType].currentCharges--;
			}
		});
		if (m_deployedMinionsUI[0].isReadyForDeploy && m_deployedMinionsUI[0].isMinion) {
			m_deployedMinionsUI[0].Deploy();
			m_targetMaraudStructure.AddDeployedItem(m_deployedMinionsUI[0]);
		}
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