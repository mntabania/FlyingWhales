using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using Ruinarch;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class SkillUpgradeUIController : MVCUIController, SkillUpgradeUIView.IListener {

	enum SKILL_VIEW { AFFLICTIONS = 0, SPELLS, PLAYER_ACTION }
	[SerializeField]
	private SkillUpgradeUIModel m_skillUpgradeUIModel;
	private SkillUpgradeUIView m_skillUpgradeUIView;

	[SerializeField]
	private SkillUpgradeItemUI m_purchaseSkillItemUI; //item to instantiate
	private List<SkillUpgradeItemUI> m_skillItems = new List<SkillUpgradeItemUI>();

	private SKILL_VIEW m_currentView = SKILL_VIEW.AFFLICTIONS;

	public FakePlayer fakePlayer;

	public bool isTestScene;
	private bool m_isInitialized = false;

	private PlayerSkillComponent m_skillComponent;

	private void Start() {
		if (!isTestScene) {
			if (UIManager.Instance != null) {
				UIManager.Instance.onSpireClicked += OnSpireClicked;
			}
		} else {
			Init();
		}
		SkillUpgradeItemUI.onHoverOverUpgradeItem += OnHoverOverUpgradeItem;
		SkillUpgradeItemUI.onHoverOutUpgradeItem += OnHoverOutUpgradeItem;
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onSpireClicked -= OnSpireClicked;
		}
		SkillUpgradeItemUI.onHoverOverUpgradeItem -= OnHoverOverUpgradeItem;
		SkillUpgradeItemUI.onHoverOutUpgradeItem -= OnHoverOutUpgradeItem;
	}

	private void OnSpireClicked() {
		if (GameManager.Instance.gameHasStarted) {
			if (!m_isInitialized) {
				Init();
				m_isInitialized = true;
			} else {
				InstantiateUI();
			}
		}
	}

	public void Init() {
		ListPoolManager.Initialize();
		if (isTestScene) {
			fakePlayer.Initialize();
			m_skillComponent = fakePlayer.skillComponent;
			InstantiateUI();
		} else {
			m_skillComponent = PlayerManager.Instance.player.playerSkillComponent;
			InstantiateUI();
		}
	}

	public void Open() { 
		ShowUI();
		Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}

	#region Listeners
	private void OnPlaguePointsUpdated(int p_plaguePoints) {
		UpdateTopMenuSummary();
	}
	#endregion

	public override void ShowUI() {
		base.ShowUI();
		UpdateTopMenuSummary();
		m_skillUpgradeUIView.SetTransmissionTabIsOnWithoutNotify(true);
		InputManager.Instance.SetAllHotkeysEnabledState(false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(KeyCode.Escape, true);
		GameManager.Instance.SetPausedState(true);
	}
	public override void HideUI() {
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(true);
		base.HideUI();
		Messenger.RemoveListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnPlaguePointsUpdated);
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_skillUpgradeUIView == null) {
			SkillUpgradeUIView.Create(_canvas, m_skillUpgradeUIModel, (p_ui) => {
				m_skillUpgradeUIView = p_ui;
				m_skillUpgradeUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				ShowUI();
				DisplaySkills(GetFilteredAfflictions());
			});
		} else {
			ShowUI();
			DisplaySkills(GetFilteredAfflictions());
		}
	}

	void DisplaySkills(List<PLAYER_SKILL_TYPE> p_skillSets) {
		int plaguePoints;
		if (isTestScene) {
			plaguePoints = fakePlayer.currenciesComponent.Spirits;
		} else {
			plaguePoints = PlayerManager.Instance.player.plagueComponent.plaguePoints;
		}
		ClearListFirst();
		List<PLAYER_SKILL_TYPE> skillToBeDisplayed = ListPoolManager.CreateNewPlayerSkillTypeList();
		//add the available first
		p_skillSets.ForEach((eachSkill) => {
			if (!skillToBeDisplayed.Contains(eachSkill)) {
				skillToBeDisplayed.Add(eachSkill);
			}
		});
		
		SpawnSkillItems(skillToBeDisplayed);
		ListPoolManager.ReturnPlayerSkillTypeListToPool(skillToBeDisplayed);
		skillToBeDisplayed.Clear();
		UpdateTopMenuSummary();
	}

	private void UpdateTopMenuSummary() {
		int totalPowersCount = m_skillComponent.afflictions.Count + m_skillComponent.spells.Count + GetFilteredPlayerActions().Count;
		m_skillUpgradeUIView.SetUnlockSkillCount(totalPowersCount.ToString());
		if (isTestScene) {
			m_skillUpgradeUIView.SetChaticEnergyCount(fakePlayer.currenciesComponent.Spirits.ToString());
		} else {
			m_skillUpgradeUIView.SetChaticEnergyCount(PlayerManager.Instance.player.plagueComponent.plaguePoints.ToString());
		}
	}

	void ClearListFirst() {
		if (m_skillItems != null && m_skillItems.Count > 0) {
			m_skillItems.ForEach((eachItem) => {
				eachItem.onButtonClick -= OnSkillClick;
				eachItem.gameObject.SetActive(false);
			});
		}
	}

	private void SpawnSkillItems(List<PLAYER_SKILL_TYPE> listOfSkills) {
		for (int x = 0; x < listOfSkills.Count; ++x) {
			if (x < m_skillItems.Count) {
				SkillData data = PlayerSkillManager.Instance.GetSkillData(listOfSkills[x]);
				m_skillItems[x].gameObject.SetActive(true);
				if (isTestScene) {
					m_skillItems[x].InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					m_skillItems[x].InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
				}
				m_skillItems[x].onButtonClick += OnSkillClick;
			} else {
				SkillUpgradeItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI, m_skillUpgradeUIView.GetSkillParent());
				go.transform.localScale = new Vector3(1f, 1f, 1f);
				SkillData data = PlayerSkillManager.Instance.GetSkillData(listOfSkills[x]);
				if (isTestScene) {
					go.InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					go.InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
				}
				
				go.onButtonClick += OnSkillClick;
				m_skillItems.Add(go);
			}
		}
	}

	void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		PlayerSkillData data = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
		if (isTestScene) {
			fakePlayer.currenciesComponent.AdjustPlaguePoints(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		} else {
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		}
		skillData.LevelUp();
		switch (m_currentView) {
			case SKILL_VIEW.AFFLICTIONS:
			DisplaySkills(GetFilteredAfflictions());
			break;
			case SKILL_VIEW.SPELLS:
			DisplaySkills(GetFilteredSpells());
			break;
			case SKILL_VIEW.PLAYER_ACTION:
			DisplaySkills(GetFilteredPlayerActions());
			break;
		}
		UpdateTopMenuSummary();
		//m_skillComponent.SetPlayerSkillData(p_type);

		//Broadcast is moved inside LevelUp function
		//Messenger.Broadcast(SpellSignals.PLAYER_SKILL_UPGRADED_ON_SPIRE, skillData);
	}

	#region SkillUpgradeUIView.IListener implementation
	public void OnAfflictionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.AFFLICTIONS;
			DisplaySkills(GetFilteredAfflictions());
			UpdateTopMenuSummary();
		}
	}
	public void OnSpellTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.SPELLS;
			DisplaySkills(GetFilteredSpells());
			UpdateTopMenuSummary();
		}
	}
	public void OnPlayerActionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.PLAYER_ACTION;
			DisplaySkills(GetFilteredPlayerActions());
			UpdateTopMenuSummary();
		}
	}

	public List<PLAYER_SKILL_TYPE> GetFilteredAfflictions() {
		List<PLAYER_SKILL_TYPE> skills = new List<PLAYER_SKILL_TYPE>();
		m_skillComponent.afflictions.ForEach((eachSkill) => {
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(eachSkill).isNonUpgradeable) {
				skills.Add(eachSkill);
			}
		});
		return skills;
	}
	public List<PLAYER_SKILL_TYPE> GetFilteredPlayerActions() {
		List<PLAYER_SKILL_TYPE> skills = new List<PLAYER_SKILL_TYPE>();
		m_skillComponent.playerActions.ForEach((eachSkill) => {
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(eachSkill).isNonUpgradeable) {
				skills.Add(eachSkill);
			}
		});
		return skills;
	}
	public List<PLAYER_SKILL_TYPE> GetFilteredSpells() {
		List<PLAYER_SKILL_TYPE> skills = new List<PLAYER_SKILL_TYPE>();
		m_skillComponent.spells.ForEach((eachSkill) => {
			if (!PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(eachSkill).isNonUpgradeable) {
				skills.Add(eachSkill);
			}
		});
		return skills;
	}
	
	public void OnCloseClicked() {
		HideUI();
	}
	//public void OnHoveredOverPlaguedRat(UIHoverPosition p_hoverPosition) {
	//	if (UIManager.Instance != null && PlayerManager.Instance != null) {
	//		if (PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.BIOLAB) is Biolab biolab && !biolab.HasMaxPlaguedRat()) {
	//			string timeDifference = GameManager.Instance.Today().GetTimeDifferenceString(biolab.replenishDate);
	//			string summary = $"The Biolab produces a Plagued Rat once every 2 days up to a maximum of \n3 charges. A new Plagued Rat charge will be produced in {UtilityScripts.Utilities.ColorizeAction(timeDifference)}.";
	//			UIManager.Instance.ShowSmallInfo(summary, p_hoverPosition, "Plagued Rats");
	//		}
	//	}
	//}
	//public void OnHoveredOutPlaguedRat() {
	//	if (UIManager.Instance != null && PlayerManager.Instance != null) {
	//		UIManager.Instance.HideSmallInfo();
	//	}
	//}
	#endregion

	#region Tooltips
	private void OnHoverOverUpgradeItem(PLAYER_SKILL_TYPE p_skillType) {
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_skillType);
		if (skillData.isMaxLevel) {
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillDetails(skillData, m_skillUpgradeUIView.UIModel.tooltipPosition, true);
		} else {
			PlayerSkillData playerData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
			bool isChaoticEnergyEnough = PlayerManager.Instance.player.chaoticEnergy >= playerData.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel);
			PlayerUI.Instance.skillDetailsTooltip.ShowPlayerSkillWithLevelUpDetails(skillData, m_skillUpgradeUIView.UIModel.tooltipPosition, isChaoticEnergyEnough);	
		}
	}
	private void OnHoverOutUpgradeItem(PLAYER_SKILL_TYPE p_skillType) {
		PlayerUI.Instance.skillDetailsTooltip.HidePlayerSkillDetails();
	}
	#endregion
}