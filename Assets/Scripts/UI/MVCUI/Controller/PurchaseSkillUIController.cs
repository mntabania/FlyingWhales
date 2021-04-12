using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using Ruinarch;
using UtilityScripts;

public class PurchaseSkillUIController : MVCUIController, PurchaseSkillUIView.IListener {

	public bool isTestScene;
	public int skillCountPerDraw = 3;
	[SerializeField]
	private PurchaseSkillUIModel m_purchaseSkillUIModel;
	private PurchaseSkillUIView m_purchaseSkillUIView;

	[SerializeField]
	private PurchaseSkillItemUI m_purchaseSkillItemUI; //item to instantiate

	private WeightedDictionary<SkillData> m_weightedList;
	private int m_numberOfSkills;
	private SkillProgressionManager m_skillProgressionManager = new SkillProgressionManager();

	public FakePlayer fakePlayer;

	private GameDate m_nextPurchased;
	
	private bool m_firstRun = true;
	private bool m_isDrawn;
	private string m_tooltipCancelReleaseAbility;
	
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
		SubscribeListeners();
	}

	private void OnDestroy() {
		if (m_purchaseSkillUIView != null && m_purchaseSkillUIView.UIModel != null) {
			m_purchaseSkillUIView.UIModel.skillItems.ForEach(CleanupItem);	
		}
	}
	private void CleanupItem(PurchaseSkillItemUI eachItem) {
		eachItem.onButtonClick -= OnSkillClick;
		eachItem.onHoverOver -= OnHoverOverSkill;
		eachItem.onHoverOut -= OnHoverOutSkill;
	}
	#endregion
	
	public void InitializeAfterLoadoutSelected() {
		m_tooltipCancelReleaseAbility = LocalizationManager.Instance.GetLocalizedValue("UI", "PortalUI", "cancel_release_ability");
		Init(skillCountPerDraw, false);
		HideUI();
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetTimer(PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell);
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetHoverOverAction(OnHoverOverReleaseAbilityTimer);
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.SetHoverOutAction(OnHoverOutReleaseAbilityTimer);
	}
	private void SubscribeListeners() {
		Messenger.AddListener<PLAYER_SKILL_TYPE, int>(PlayerSignals.PLAYER_FINISHED_SKILL_UNLOCK, OnPlayerFinishedSkillUnlock);
		Messenger.AddListener<SkillData, int>(PlayerSignals.PLAYER_CHOSE_SKILL_TO_UNLOCK, OnPlayerChoseSkillToUnlock);
		Messenger.AddListener(PlayerSignals.PLAYER_SKILL_UNLOCK_CANCELLED, OnPlayerCancelledSkillUnlock);
	}
	private void OnPlayerFinishedSkillUnlock(PLAYER_SKILL_TYPE p_skill, int p_unlockCost) {
		OnFinishSkillUnlock();
	}
	private void OnPlayerChoseSkillToUnlock(SkillData p_skill, int p_unlockCost) {
		UpdateTimerState();
		UpdateWindowCoverState();
		m_purchaseSkillUIView.UIModel.timerReleaseAbility.RefreshName();
	}
	private void OnPlayerCancelledSkillUnlock() {
		UpdateTimerState();
		UpdateWindowCoverState();
		UpdateRerollBtn();
		UpdateItems();
	}
	public override void HideUI() {
		base.HideUI();
		UIManager.Instance.SetSpeedTogglesState(true);
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(true);
		InnerMapCameraMove.Instance.EnableMovement();
	}
	public override void ShowUI() {
		m_mvcUIView.ShowUI();
	}
	bool GetIsAvailable() { 
		return GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) <= 0;
	}

	public void Init(int numberOfSkills, bool playShowAnimation) {
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
		if (PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.IsFinished()) {
			m_purchaseSkillUIView.SetRerollCooldownFill(0f);
		} else {
			m_purchaseSkillUIView.SetRerollCooldownFill(1f - PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetCurrentTimerProgressPercent());	
		}
		UpdateWindowCoverState();
		UpdateTimerState();
		if (playShowAnimation) {
			m_purchaseSkillUIView.PlayShowAnimation();	
		}
	}
	private void UpdateWindowCoverState() {
		if (PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.IsFinished()) {
			m_purchaseSkillUIView.SetWindowCoverState(false);
		} else {
			m_purchaseSkillUIView.SetWindowCoverState(true);
		}
	}
	private void UpdateTimerState() {
		m_purchaseSkillUIView.SetTimerState(!PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.IsFinished());
	}
	private void SpawnItems() {
		if (m_weightedList.Count <= 0) {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
		} else {
			m_purchaseSkillUIView.UIModel.skillItems.ForEach((eachSkill) => {
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
			
			if (m_purchaseSkillUIView.UIModel.skillItems.Count <= 0) {
				for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count; i++) {
					PLAYER_SKILL_TYPE skillType = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices[i];
					SkillData data = PlayerSkillManager.Instance.GetSkillData(skillType);
					PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI, m_purchaseSkillUIView.GetSkillsParent(), true);
					go.InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
					go.onButtonClick += OnSkillClick;
					go.onHoverOver += OnHoverOverSkill;
					go.onHoverOut += OnHoverOutSkill;
					m_purchaseSkillUIView.UIModel.skillItems.Add(go);
				}
			} else {
				for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count; i++) {
					PLAYER_SKILL_TYPE skillType = PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices[i];
					SkillData data = PlayerSkillManager.Instance.GetSkillData(skillType);
					PurchaseSkillItemUI skillItem = m_purchaseSkillUIView.UIModel.skillItems[i];
					skillItem.gameObject.SetActive(true);
					skillItem.InitItem(data.type, PlayerManager.Instance.player.plagueComponent.plaguePoints);
				}
			}
			m_weightedList.Clear();
		}
	}
	private void UpdateItems() {
		if (m_purchaseSkillUIView.UIModel.skillItems.Count > 0) {
			m_purchaseSkillUIView.UIModel.skillItems.ForEach((eachItems) => {
				eachItems.UpdateItem(PlayerManager.Instance.player.plagueComponent.plaguePoints);
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
					PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(entry.Value.type);
					if (playerSkillData != null) {
						if (m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.plagueComponent.plaguePoints, entry.Value.type) != -1) {
							int processedWeight = playerSkillData.baseLoadoutWeight;
							if (PlayerSkillManager.Instance.selectedArchetype == playerSkillData.archetypeWeightedBonus) {
								processedWeight += 100;
							}
							if (processedWeight > 0) {
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
			InputManager.Instance.SetAllHotkeysEnabledState(false);
			InnerMapCameraMove.Instance.DisableMovement();
		});
	}

	private void DisplayMenu() {
		if (GetIsAvailable()) {
			if (!m_isDrawn) {
				UpdateRerollBtn();
				MakeListForAvailableSkills();
				SpawnItems();
				m_purchaseSkillUIView.ShowSkills();
				UpdateItems();
			} else {
				if (m_weightedList.Count > 0 || PlayerManager.Instance.player.playerSkillComponent.currentSpellChoices.Count > 0) {
					UpdateRerollBtn();
					m_purchaseSkillUIView.ShowSkills();
					UpdateItems();	
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
		InputManager.Instance.SetAllHotkeysEnabledState(false);
		InnerMapCameraMove.Instance.DisableMovement();
	}

	#region Reroll
	private void UpdateRerollBtn() {
		if (skillComponentToUse.cooldownReroll.IsFinished() && skillComponentToUse.timerUnlockSpell.IsFinished()) {
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
		MakeListForAvailableSkills();
		UpdateRerollBtn();
		SpawnItems();
		m_purchaseSkillUIView.ShowSkills();
		m_purchaseSkillUIView.PlayItemsAnimation();
	}
	public void OnCloseClicked() {
		m_purchaseSkillUIView.PlayHideAnimation(HideUI);
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
	public void OnClickCancelReleaseAbility() {
		SkillData spellData = PlayerSkillManager.Instance.GetSkillData(PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked);
		UIManager.Instance.ShowYesNoConfirmation(
			"Cancel Release Ability", $"Are you sure you want to cancel Releasing Ability: <b>{spellData.name}</b>? " + 
			                          $"\n<i>{UtilityScripts.Utilities.InvalidColorize("Cancelling will reset all current release progress!")}</i>", OnConfirmCancelRelease, showCover: true, layer: 30);
	}
	private void OnConfirmCancelRelease() {
		PlayerManager.Instance.player.playerSkillComponent.CancelCurrentPlayerSkillUnlock();
	}
	private void OnHoverOverSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item) {
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked == PLAYER_SKILL_TYPE.NONE) {
			if (PlayerManager.Instance.player.plagueComponent.plaguePoints < p_skillData.unlockCost) {
				UIManager.Instance.ShowSmallInfo("Not enough mana!");
			} else {
				p_item.borderShineEffect.Play();
			}	
		}
	}
	private void OnHoverOutSkill(PlayerSkillData p_skillData, PurchaseSkillItemUI p_item) {
		if (PlayerManager.Instance.player.playerSkillComponent.currentSpellBeingUnlocked == PLAYER_SKILL_TYPE.NONE) {
			if (PlayerManager.Instance.player.plagueComponent.plaguePoints < p_skillData.unlockCost) {
				UIManager.Instance.HideSmallInfo();
			} else {
				p_item.borderShineEffect.Stop(true);
			}
		}
	}
	private void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		int result = isTestScene ? 
			m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(fakePlayer.skillComponent, fakePlayer.currenciesComponent, p_type) : 
			m_skillProgressionManager.CheckRequirementsAndGetUnlockCost(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.plagueComponent.plaguePoints, p_type);
		if (result != -1) {
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_type);
			m_firstRun = false;
			PlayerManager.Instance.player.plagueComponent.AdjustPlaguePoints(-result);
			PlayerManager.Instance.player.playerSkillComponent.PlayerChoseSkillToUnlock(skillData, result);
			UpdateRerollBtn();
			UpdateItems();
		}
	}
	public void OnFinishSkillUnlock() {
		MakeListForAvailableSkills();
		m_isDrawn = false;
	}
	#endregion

	#region Tooltips
	private void OnHoverOverReleaseAbilityTimer() {
		string message = $"Remaining time: {PlayerManager.Instance.player.playerSkillComponent.timerUnlockSpell.GetRemainingTimeString()}";
		// if (PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.IsFinished()) {
		// 	message = $"{message}\nReroll Available!";  
		// } else {
		// 	message = $"{message}\nRemaining time until reroll: {PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetRemainingTimeString()}";
		// }
		UIManager.Instance.ShowSmallInfo(message, autoReplaceText: false);
	}
	private void OnHoverOutReleaseAbilityTimer() {
		UIManager.Instance.HideSmallInfo();
	}
	public void OnHoverOverCancelReleaseAbility() {
		UIManager.Instance.ShowSmallInfo(m_tooltipCancelReleaseAbility);
	}
	public void OnHoverOutCancelReleaseAbility() {
		UIManager.Instance.HideSmallInfo();
	}
	#endregion
}