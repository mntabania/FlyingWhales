using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using UtilityScripts;

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

	private GameDate m_nextPurchased;
	
	private bool m_firstRun = true;
	private bool m_isDrawn;
	
	#region getters
	private PlayerSkillComponent skillComponentToUse {
		get {
			if (isTestScene) {
				return fakePlayer.skillComponent;
			}
			return PlayerManager.Instance.player.playerSkillComponent;
		}
	}
	#endregion
	
	#region mono
	private void Start() {
		if (isTestScene) {
			Init(skillCountPerDraw);
		} else {
			// UIManager.Instance.onPortalClicked += OnPortalClicked;
		}
	}

	private void OnDestroy() {
		// if (UIManager.Instance != null) {
		// 	UIManager.Instance.onPortalClicked -= OnPortalClicked;
		// }
		m_skillItems.ForEach(CleanupItem);
	}
	private void CleanupItem(PurchaseSkillItemUI eachItem) {
		eachItem.onButtonClick -= OnSkillClick;
		eachItem.onHoverOver -= OnHoverOverSkill;
		eachItem.onHoverOut -= OnHoverOutSkill;
	}
	#endregion

	public override void HideUI() {
		base.HideUI();
		UIManager.Instance.SetSpeedTogglesState(true);
		UIManager.Instance.ResumeLastProgressionSpeed();
	}
	public override void ShowUI() {
		m_mvcUIView.ShowUI();
	}
	bool GetIsAvailable() { 
		return GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) <= 0;
	}

	public void Init(int numberOfSkills) {
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
		if (PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.IsFinished()) {
			m_purchaseSkillUIView.SetRerollCooldownFill(0f);
		} else {
			m_purchaseSkillUIView.SetRerollCooldownFill(1f - PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetCurrentTimerProgressPercent());	
		}
		
	}

	// private void OnPortalClicked() {
	// 	if (GameManager.Instance.gameHasStarted) {
	// 		Init(skillCountPerDraw);
	// 	}
	// }
	private void SpawnItems() {
		if (m_weightedList.Count <= 0) {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
		} else {
			m_skillItems.ForEach((eachSkill) => {
				eachSkill.gameObject.SetActive(false);
			});
			int count = m_numberOfSkills;
			if (m_weightedList.Count < count) {
				count = m_weightedList.Count;
			}
			if (PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count <= 0) {
				//randomize from weights if no choices have been set.
				for (int i = 0; i < count; i++) {
					SkillData data = m_weightedList.PickRandomElementGivenWeights();
					m_weightedList.RemoveElement(data);
					PlayerManager.Instance.player.playerSkillComponent.AddCurrentPlayerSpellChoice(data.type);
				}
			}
			
			
			if (m_skillItems.Count <= 0) {
				for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count; i++) {
					PLAYER_SKILL_TYPE skillType = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices[i];
					SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(skillType);
					PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI, m_purchaseSkillUIView.GetSkillsParent(), true);
					go.InitItem(data.type, PlayerManager.Instance.player.mana);
					go.onButtonClick += OnSkillClick;
					go.onHoverOver += OnHoverOverSkill;
					go.onHoverOut += OnHoverOutSkill;
					m_skillItems.Add(go);
				}
				//AddTestSkill(PLAYER_SKILL_TYPE.PLAGUE);
				// for (int x = 0; x < m_numberOfSkills; ++x) {
				// 	PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI, m_purchaseSkillUIView.GetSkillsParent(), true);
				// 	SkillData data = m_weightedList.PickRandomElementGivenWeights();
				// 	go.InitItem(data.type, PlayerManager.Instance.player.mana);
				// 	go.onButtonClick += OnSkillClick;
				// 	m_skillItems.Add(go);
				// 	m_weightedList.RemoveElement(data);
				// }
			} else {
				for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count; i++) {
					PLAYER_SKILL_TYPE skillType = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices[i];
					SkillData data = PlayerSkillManager.Instance.GetPlayerSkillData(skillType);
					PurchaseSkillItemUI skillItem = m_skillItems[i];
					skillItem.gameObject.SetActive(true);
					skillItem.InitItem(data.type, PlayerManager.Instance.player.mana);
				}
				// for (int x = 0; x < count; ++x) {
				// 	/* should remove if everything has a weight */
				// 	if (m_weightedList.Count <= 0) {
				// 		break;
				// 	}
				// 	SkillData data = m_weightedList.PickRandomElementGivenWeights();
				// 	if (data == null) { /* should remove if everything has a weight */
				// 		m_weightedList.RemoveElement(data);
				// 		--x;
				// 		continue;
				// 	}
				// 	m_skillItems[x].gameObject.SetActive(true);
				// 	m_skillItems[x].InitItem(data.type, PlayerManager.Instance.player.mana);
				// 	m_weightedList.RemoveElement(data);
				// }
			}
			m_weightedList.Clear();
		}
	}
	private void UpdateItem() {
		if (m_skillItems.Count > 0) {
			m_skillItems.ForEach((eachItems) => {
				eachItems.UpdateItem(PlayerManager.Instance.player.mana);
			});
		}
	}
	private void MakeListForAvailableSkills() {
		m_weightedList = TryGetWeightedSpellChoicesList();
		m_isDrawn = true;
		Debug.Log(m_weightedList.GetWeightsSummary("Set weighted list to: "));
	}
	private WeightedDictionary<SkillData> TryGetWeightedSpellChoicesList() {
		WeightedDictionary<SkillData> weights = new WeightedDictionary<SkillData>();
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> entry in PlayerSkillManager.Instance.allPlayerSkillsData) {
			if (!entry.Value.isInUse) {
				if (entry.Value.category == PLAYER_SKILL_CATEGORY.AFFLICTION || entry.Value.category == PLAYER_SKILL_CATEGORY.PLAYER_ACTION || entry.Value.category == PLAYER_SKILL_CATEGORY.SPELL) {
					PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(entry.Value.type);
					if (playerSkillData != null) {
						if (m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.mana, entry.Value.type) != -1) {
							int processedWeight = playerSkillData.baseLoadoutWeight;
							if (PlayerSkillManager.Instance.selectedArchetype == playerSkillData.archetypeWeightedBonus) {
								processedWeight += 100;
							}
							if (processedWeight >= 0) {
								weights.AddElement(entry.Value, processedWeight);
							}
						}
					}	
				}
			}
		}
		return weights;
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (GetIsAvailable() || m_firstRun) {
			if (m_purchaseSkillUIView == null) { //first run
				//PlayerSkillManager.Instance.Initialize();
				MakeListForAvailableSkills();
				DisplayMenuFirstTime();
			} else {
				DisplayMenu();
				ShowUI();
			}
		} else {
			ShowUI();
			DisplayMenu();
		}
	}

	private void DisplayMenuFirstTime() {
		PurchaseSkillUIView.Create(_canvas, m_purchaseSkillUIModel, (p_ui) => {
			m_purchaseSkillUIView = p_ui;
			m_purchaseSkillUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 2;
			m_purchaseSkillUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);
			ShowUI();
			m_purchaseSkillUIView.ShowSkills();
			UpdateRerollBtn();
			SpawnItems();
			UIManager.Instance.Pause();
			UIManager.Instance.SetSpeedTogglesState(false);
		});
	}

	private void DisplayMenu() {
		if (GetIsAvailable()) {
			if (!m_isDrawn) {
				UpdateRerollBtn();
				MakeListForAvailableSkills();
				SpawnItems();
				m_purchaseSkillUIView.ShowSkills();
				UpdateItem();
			} else {
				if (m_weightedList.Count > 0 || PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count > 0) {
					UpdateRerollBtn();
					m_purchaseSkillUIView.ShowSkills();
					UpdateItem();	
				} else {
					m_purchaseSkillUIView.HideSkills();
					m_purchaseSkillUIView.DisableRerollButton();
					m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
				}
				
			}
		} else {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + (GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased)) + " ticks");
		}
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(false);
	}

	#region Reroll
	private void UpdateRerollBtn() {
		if (skillComponentToUse.cooldownReroll.IsFinished()) {
			m_purchaseSkillUIView.EnableRerollButton();	
		} else {
			m_purchaseSkillUIView.DisableRerollButton();
		}
	}
	#endregion
	
	#region Listeners
	public void OnRerollClicked() {
		m_firstRun = false;
		PlayerManager.Instance.player.playerSkillComponent.OnRerollUsed();
		// m_nextPurchased = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
		MakeListForAvailableSkills();
		UpdateRerollBtn();
		SpawnItems();
		// m_isDrawn = false;
		m_purchaseSkillUIView.ShowSkills();
		// m_purchaseSkillUIView.HideSkills();
		// m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + (GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased)) + " ticks");
	}
	public void OnCloseClicked() {
		HideUI();
	}
	public void OnHoverOverReroll() {
		if (!m_purchaseSkillUIView.UIModel.btnReroll.IsInteractable()) {
			if (!PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.IsFinished()) {
				string tooltip = LocalizationManager.Instance.GetLocalizedValue("UI", "PurchaseSkillUI", "reroll_tooltip_cooldown");
				List<LogFillerStruct> fillers = RuinarchListPool<LogFillerStruct>.Claim();
				fillers.Add(new LogFillerStruct(null, PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetRemainingTimeString(), LOG_IDENTIFIER.STRING_1));
				tooltip = UtilityScripts.Utilities.StringReplacer(tooltip, fillers);
				UIManager.Instance.ShowSmallInfo(tooltip, autoReplaceText: false);
				RuinarchListPool<LogFillerStruct>.Release(fillers);	
			}
		} else {
			string tooltip = LocalizationManager.Instance.GetLocalizedValue("UI", "PurchaseSkillUI", "reroll_tooltip_default");
			List<LogFillerStruct> fillers = RuinarchListPool<LogFillerStruct>.Claim();
			fillers.Add(new LogFillerStruct(null, PlayerSkillComponent.RerollCooldownInHours.ToString(), LOG_IDENTIFIER.STRING_1));
			tooltip = UtilityScripts.Utilities.StringReplacer(tooltip, fillers);
			UIManager.Instance.ShowSmallInfo(tooltip, autoReplaceText: false);
			RuinarchListPool<LogFillerStruct>.Release(fillers);
		}
	}
	public void OnHoverOutReroll() {
		UIManager.Instance.HideSmallInfo();
	}
	private void OnHoverOverSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item) {
		if (PlayerManager.Instance.player.mana < p_skillData.unlockCost) {
			UIManager.Instance.ShowSmallInfo("Not enough mana!");
		} else {
			p_item.shineEffect.Play();
		}
	}
	private void OnHoverOutSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item) {
		if (PlayerManager.Instance.player.mana < p_skillData.unlockCost) {
			UIManager.Instance.HideSmallInfo();
		} else {
			p_item.shineEffect.Stop(true);
		}
	}
	private void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		int result = isTestScene ? 
			m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(fakePlayer.skillComponent, fakePlayer.currenciesComponent, p_type) : 
			m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.mana, p_type);
		if (result != -1) {
			SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
			m_firstRun = false;
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			
			
			PlayerManager.Instance.player.AdjustMana(-result);
			PlayerManager.Instance.player.playerSkillComponent.PlayerChoseSkillToUnlock(skillData, result);
			
			HideUI();
			return;
			MakeListForAvailableSkills();
			m_firstRun = false;
			m_nextPurchased = GameManager.Instance.Today().AddDays(1);
			PlayerManager.Instance.player.AdjustMana(-result);
			// PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(p_type);
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			// if (skillData.category == PLAYER_SKILL_CATEGORY.SPELL) {
			// 	Messenger.Broadcast(SpellSignals.PLAYER_GAINED_SPELL, p_type);
			// }
			m_isDrawn = false;
			m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + (GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased)) + " ticks");
		}
	}
	public void OnFinishSkillUnlock() {
		MakeListForAvailableSkills();
		m_isDrawn = false;
	}
	#endregion
}