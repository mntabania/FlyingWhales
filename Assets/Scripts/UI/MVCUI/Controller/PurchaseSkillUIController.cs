using UnityEngine;
using Ruinarch.MVCFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class PurchaseSkillUIController : MVCUIController, PurchaseSkillUIView.IListener {

	public bool isTestScene;
	public int skillCountPerDraw = 3;
	[SerializeField]
	private PurchaseSkillUIModel m_purchaseSkillUIModel;
	private PurchaseSkillUIView m_purchaseSkillUIView;

	[SerializeField]
	private PurchaseSkillItemUI m_purchaseSkillItemUI; //item to instantiate
	private List<PurchaseSkillItemUI> m_skillItems = new List<PurchaseSkillItemUI>();

	private WeightedDictionary<SkillData> m_weightedList;
	private int m_numberOfSkills;
	private SkillProgressionManager m_skillProgressionManager = new SkillProgressionManager();

	public FakePlayer fakePlayer;

	private int m_currentDay = -1;
	private int m_dayPurchased = -1;
	private int m_dayRerolled = -1;

	private bool m_doRandomize;

	private void OnEnable() {
		Messenger.AddListener(Signals.DAY_STARTED, OnChangeDay);
	}
	public void Init(int numberOfSkills) {
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
	}

	private void OnChangeDay() {
		if (GameManager.Instance.continuousDays > m_currentDay) {
			m_currentDay = GameManager.Instance.continuousDays;
			m_doRandomize = true;
		}
	}
	#region Listeners
	public void OnRerollClicked() {
		if (m_dayRerolled != GameManager.Instance.continuousDays) {
			m_dayRerolled = GameManager.Instance.continuousDays;
			MakeListForAvailableSkills();
			SpawnItems();
		}
	}
	public void OnCloseClicked() {
		HideUI();
	}
	#endregion

	public override void ShowUI() {
		base.ShowUI();
	}
	public override void HideUI() {
		base.HideUI();
	}
	private void Start() {
		if (isTestScene) {
			Init(skillCountPerDraw);
		} else {
			UIManager.Instance.onPortalClicked += OnPortalClicked;
		}
	}

	private void OnDestroy() {
		UIManager.Instance.onPortalClicked -= OnPortalClicked;
		m_skillItems.ForEach((eachItem) => eachItem.onButtonClick -= OnSkillClick);
	}

	private void OnPortalClicked() {
		Init(skillCountPerDraw);
	}

	public void SpawnItems() {
		if (m_weightedList.Count > 0) {
			m_skillItems.ForEach((eachSkill) => {
				eachSkill.gameObject.SetActive(false);
			});
			int count = m_numberOfSkills;
			if (m_weightedList.Count < count) {
				count = m_weightedList.Count;
			}

			if (m_skillItems.Count <= 0) {
				//AddTestSkill(PLAYER_SKILL_TYPE.PLAGUE);
				for (int x = 0; x < m_numberOfSkills; ++x) {
					PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI);
					SkillData data = m_weightedList.PickRandomElementGivenWeights();
					go.InitItem(data.type);
					go.onButtonClick += OnSkillClick;
					go.transform.SetParent(m_purchaseSkillUIView.GetSkillsParent());
					m_skillItems.Add(go);
					m_weightedList.RemoveElement(data);
				}
			} else {
				for (int x = 0; x < count; ++x) {
					m_skillItems[x].gameObject.SetActive(true);
					SkillData data = m_weightedList.PickRandomElementGivenWeights();
					m_skillItems[x].InitItem(data.type);
					m_weightedList.RemoveElement(data);
				}
			}
			m_weightedList.Clear();
		}

	}

	public void AddTestSkill(PLAYER_SKILL_TYPE p_type) {
		PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI);
		go.InitItem(p_type);
		go.onButtonClick += OnSkillClick;
		go.transform.SetParent(m_purchaseSkillUIView.GetSkillsParent());
		m_skillItems.Add(go);
	}

	public void MakeListForAvailableSkills() {
		PlayerSkillManager.Instance.Initialize();
		m_weightedList = new WeightedDictionary<SkillData>();
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> entry in PlayerSkillManager.Instance.allPlayerSkillsData) {
			if (!entry.Value.isInUse) {
				PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(entry.Value.type);
				if (playerSkillData != null) {
					int processedWeight = playerSkillData.baseLoadoutWeight;
					if (PlayerSkillManager.Instance.selectedArchetype == playerSkillData.archetypeWeightedBonus) {
						processedWeight += 100;
					}
					if (processedWeight >= 0) {
						m_weightedList.AddElement(entry.Value, processedWeight);
					}
				}
			}
		}
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_dayPurchased != GameManager.Instance.continuousDays) {
			if (m_purchaseSkillUIView == null) {
				DisplayMenuFirstTime();
			} else {
				if (m_doRandomize) {
					DiplayMenu();
				}
				ShowUI();
			}
		} else {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.SetMessage("Please come back next day");
		}
	}

	private void DisplayMenuFirstTime() {
		PurchaseSkillUIView.Create(_canvas, m_purchaseSkillUIModel, (p_ui) => {
			m_purchaseSkillUIView = p_ui;
			m_purchaseSkillUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			ShowUI();
			m_purchaseSkillUIView.ShowSkills();
			MakeListForAvailableSkills();
			SpawnItems();
		});
	}

	private void DiplayMenu() {
		m_purchaseSkillUIView.ShowSkills();
		MakeListForAvailableSkills();
		SpawnItems();
		m_doRandomize = false;
	}

	public void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		int result;
		if (isTestScene) {
			result = m_skillProgressionManager.CheckAndUnlock(fakePlayer.skillComponent, fakePlayer.currenciesComponent, p_type);
		} else {
			result = m_skillProgressionManager.CheckAndUnlock(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.mana, p_type);
		}
		if (result != -1) {
			m_dayPurchased = GameManager.Instance.continuousDays;
			PlayerManager.Instance.player.AdjustMana(-result);
			PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(p_type);
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.SetMessage("Please come back next day");
		}
	}
}