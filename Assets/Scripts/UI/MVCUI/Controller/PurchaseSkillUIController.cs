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
		m_skillItems.ForEach((eachItem) => eachItem.onButtonClick -= OnSkillClick);
	}
	#endregion

	bool GetIsAvailable() { 
		return GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) <= 0;
	}

	public void Init(int numberOfSkills) {
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
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

			if (m_skillItems.Count <= 0) {
				//AddTestSkill(PLAYER_SKILL_TYPE.PLAGUE);
				for (int x = 0; x < m_numberOfSkills; ++x) {
					PurchaseSkillItemUI go = GameObject.Instantiate(m_purchaseSkillItemUI, m_purchaseSkillUIView.GetSkillsParent(), true);
					SkillData data = m_weightedList.PickRandomElementGivenWeights();
					go.InitItem(data.type, PlayerManager.Instance.player.mana);
					go.onButtonClick += OnSkillClick;
					m_skillItems.Add(go);
					m_weightedList.RemoveElement(data);
				}
			} else {
				for (int x = 0; x < count; ++x) {
					/* should remove if everything has a weight */
					if (m_weightedList.Count <= 0) {
						break;
					}
					SkillData data = m_weightedList.PickRandomElementGivenWeights();
					if (data == null) { /* should remove if everything has a weight */
						m_weightedList.RemoveElement(data);
						--x;
						continue;
					}
					m_skillItems[x].gameObject.SetActive(true);
					m_skillItems[x].InitItem(data.type, PlayerManager.Instance.player.mana);
					m_weightedList.RemoveElement(data);
				}
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
		if (m_weightedList == null) {
			m_weightedList = new WeightedDictionary<SkillData>();
		}
		m_weightedList.Clear();
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
								m_weightedList.AddElement(entry.Value, processedWeight);
							}
						}
					}	
				}
			}
		}
		m_isDrawn = true;
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
			ShowUI();
			m_purchaseSkillUIView.ShowSkills();
			SpawnItems();
			GameManager.Instance.SetPausedState(true);
		});
	}

	private void DisplayMenu() {
		if (GetIsAvailable()) {
			UpdateRerollBtn();
			if (!m_isDrawn) {
				MakeListForAvailableSkills();
				SpawnItems();
				m_purchaseSkillUIView.ShowSkills();
				UpdateItem();
			} else {
				m_purchaseSkillUIView.ShowSkills();
				UpdateItem();
			}
		} else {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + (GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased)) + " ticks");
		}
		GameManager.Instance.SetPausedState(true);
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
		GameManager.Instance.SetPausedState(false);
		HideUI();
	}
	public void OnHoverOverReroll() {
		if (!m_purchaseSkillUIView.UIModel.btnReroll.IsInteractable()) {
			string tooltip = LocalizationManager.Instance.GetLocalizedValue("UI", "PurchaseSkillUI", "reroll_tooltip");
			List<LogFillerStruct> fillers = RuinarchListPool<LogFillerStruct>.Claim();
			fillers.Add(new LogFillerStruct(null, PlayerManager.Instance.player.playerSkillComponent.cooldownReroll.GetRemainingTimeString(), LOG_IDENTIFIER.STRING_1));
			tooltip = UtilityScripts.Utilities.StringReplacer(tooltip, fillers);
			UIManager.Instance.ShowSmallInfo(tooltip);
			RuinarchListPool<LogFillerStruct>.Release(fillers);
		}
	}
	public void OnHoverOutReroll() {
		if (!m_purchaseSkillUIView.UIModel.btnReroll.IsInteractable()) {
			UIManager.Instance.HideSmallInfo();
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