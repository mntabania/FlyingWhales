using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Linq;
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
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onSpireClicked -= OnSpireClicked;
		}
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
	}
	public override void HideUI() {
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
				DisplaySkills(m_skillComponent.afflictions);
			});
		} else {
			ShowUI();
			DisplaySkills(m_skillComponent.afflictions);
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
		p_skillSets.ForEach((eachSkill) => {
			if (PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(eachSkill).skillUpgradeData.
			GetUpgradeCostBaseOnLevel(PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill).currentLevel) <= plaguePoints) {
				if (!skillToBeDisplayed.Contains(eachSkill)) {
					skillToBeDisplayed.Add(eachSkill);
				}
			}
		});
		p_skillSets.ForEach((eachSkill) => {
			if (PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(eachSkill).skillUpgradeData.
			GetUpgradeCostBaseOnLevel(PlayerSkillManager.Instance.GetPlayerSkillData(eachSkill).currentLevel) > plaguePoints) {
				if (!skillToBeDisplayed.Contains(eachSkill)) {
					skillToBeDisplayed.Add(eachSkill);
				}
			}
		});
		SpawnSkillItems(skillToBeDisplayed);
		ListPoolManager.ReturnPlayerSkillTypeListToPool(skillToBeDisplayed);
		skillToBeDisplayed.Clear();
		UpdateTopMenuSummary();
	}

	private void UpdateTopMenuSummary() {
		List<PLAYER_SKILL_TYPE> skills = m_skillComponent.afflictions;
		switch (m_currentView) {
			case SKILL_VIEW.AFFLICTIONS:
			skills = m_skillComponent.afflictions;
			break;
			case SKILL_VIEW.SPELLS:
			skills = m_skillComponent.spells;
			break;
			case SKILL_VIEW.PLAYER_ACTION:
			skills = m_skillComponent.playerActions;
			break;
		}
		m_skillUpgradeUIView.SetUnlockSkillCount(skills.Count.ToString());
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
				SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(listOfSkills[x]);
				m_skillItems[x].gameObject.SetActive(true);
				if (isTestScene) {
					m_skillItems[x].InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					m_skillItems[x].InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
				}
				m_skillItems[x].onButtonClick += OnSkillClick;
			} else {
				SkillUpgradeItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI);
				SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(listOfSkills[x]);
				if (isTestScene) {
					go.InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					go.InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
				}
				
				go.onButtonClick += OnSkillClick;
				go.transform.SetParent(m_skillUpgradeUIView.GetSkillParent());
				m_skillItems.Add(go);
			}
		}
	}

	void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(p_type);
		SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
		skillData.LevelUp();
		switch (m_currentView) {
			case SKILL_VIEW.AFFLICTIONS:
			DisplaySkills(m_skillComponent.afflictions);
			break;
			case SKILL_VIEW.SPELLS:
			DisplaySkills(m_skillComponent.spells);
			break;
			case SKILL_VIEW.PLAYER_ACTION:
			DisplaySkills(m_skillComponent.playerActions);
			break;
		}
		if (isTestScene) {
			fakePlayer.currenciesComponent.AdjustPlaguePoints(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		} else { 
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		}
		UpdateTopMenuSummary();
		//m_skillComponent.SetPlayerSkillData(p_type);
		Messenger.Broadcast(SpellSignals.SPELL_UPGRADED, skillData);
	}

	#region BiolabUIView.IListener implementation
	public void OnAfflictionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.AFFLICTIONS;
			DisplaySkills(m_skillComponent.afflictions);
			UpdateTopMenuSummary();
		}
	}
	public void OnSpellTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.SPELLS;
			DisplaySkills(m_skillComponent.spells);
			UpdateTopMenuSummary();
		}
	}
	public void OnPlayerActionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.PLAYER_ACTION;
			DisplaySkills(m_skillComponent.playerActions);
			UpdateTopMenuSummary();
		}
	}
	
	public void OnCloseClicked() {
		HideUI();
	}
	public void OnHoveredOverPlaguedRat(UIHoverPosition p_hoverPosition) {
		if (UIManager.Instance != null && PlayerManager.Instance != null) {
			if (PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.BIOLAB) is Biolab biolab && !biolab.HasMaxPlaguedRat()) {
				string timeDifference = GameManager.Instance.Today().GetTimeDifferenceString(biolab.replenishDate);
				string summary = $"The Biolab produces a Plagued Rat once every 2 days up to a maximum of \n3 charges. A new Plagued Rat charge will be produced in {UtilityScripts.Utilities.ColorizeAction(timeDifference)}.";
				UIManager.Instance.ShowSmallInfo(summary, p_hoverPosition, "Plagued Rats");
			}
		}
	}
	public void OnHoveredOutPlaguedRat() {
		if (UIManager.Instance != null && PlayerManager.Instance != null) {
			UIManager.Instance.HideSmallInfo();
		}
	}
	#endregion
}