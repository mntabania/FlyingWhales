﻿using UnityEngine;
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
	private UpgradeSkillItemUI m_purchaseSkillItemUI; //item to instantiate
	private List<UpgradeSkillItemUI> m_skillItems = new List<UpgradeSkillItemUI>();

	private SKILL_VIEW m_currentView = SKILL_VIEW.AFFLICTIONS;

	public FakePlayer fakePlayer;

	public bool isTestScene;

	private PlayerSkillComponent m_skillComponent;

	void Start() {
		Init();
	}

	public void Init() {
		if (isTestScene) {
			fakePlayer.Initialize();
			m_skillComponent = fakePlayer.skillComponent;
		} else {
			PlayerManager.Instance.Initialize();
			m_skillComponent = PlayerManager.Instance.player.playerSkillComponent;
		}
		InstantiateUI();
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
		SkillUpgradeUIView.Create(_canvas, m_skillUpgradeUIModel, (p_ui) => {
			m_skillUpgradeUIView = p_ui;
			m_skillUpgradeUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			ShowUI();
			SpawnSkillItems(m_skillComponent.afflictions);
		});
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
			m_skillUpgradeUIView.SetChaticEnergyCount(PlayerManager.Instance.player.spiritEnergy.ToString());
		}
		
	}

	private void SpawnSkillItems(List<PLAYER_SKILL_TYPE> listOfSkills) {
		if (m_skillItems != null && m_skillItems.Count > 0) {
			m_skillItems.ForEach((eachItem) => {
				eachItem.onButtonClick -= OnSkillClick;
				eachItem.gameObject.SetActive(false);
			});
		}

		for (int x = 0; x < listOfSkills.Count; ++x) {
			if (x < m_skillItems.Count) {
				SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(listOfSkills[x]);
				m_skillItems[x].gameObject.SetActive(true);
				if (isTestScene) {
					m_skillItems[x].InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					m_skillItems[x].InitItem(data.type, PlayerManager.Instance.player.spiritEnergy);
				}
				m_skillItems[x].onButtonClick += OnSkillClick;
			} else {
				UpgradeSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI);
				SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(listOfSkills[x]);
				if (isTestScene) {
					go.InitItem(data.type, fakePlayer.currenciesComponent.Spirits);
				} else {
					go.InitItem(data.type, PlayerManager.Instance.player.spiritEnergy);
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
			SpawnSkillItems(m_skillComponent.afflictions);
			break;
			case SKILL_VIEW.SPELLS:
			SpawnSkillItems(m_skillComponent.spells);
			break;
			case SKILL_VIEW.PLAYER_ACTION:
			SpawnSkillItems(m_skillComponent.playerActions);
			break;
		}
		if (isTestScene) {
			fakePlayer.currenciesComponent.AdjustPlaguePoints(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		} else { 
			PlayerManager.Instance.player.AdjustSpiritEnergy(-1 * data.skillUpgradeData.GetUpgradeCostBaseOnLevel(skillData.currentLevel));
		}
		UpdateTopMenuSummary();
	}

	#region BiolabUIView.IListener implementation
	public void OnAfflictionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.AFFLICTIONS;
			SpawnSkillItems(m_skillComponent.afflictions);
			UpdateTopMenuSummary();
		}
	}
	public void OnSpellTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.SPELLS;
			SpawnSkillItems(m_skillComponent.spells);
			UpdateTopMenuSummary();
		}
	}
	public void OnPlayerActionTabClicked(bool isOn) {
		if (isOn) {
			m_currentView = SKILL_VIEW.PLAYER_ACTION;
			SpawnSkillItems(m_skillComponent.playerActions);
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