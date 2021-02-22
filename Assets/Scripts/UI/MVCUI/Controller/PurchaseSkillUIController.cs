using UnityEngine;
using Ruinarch.MVCFramework;
using System.Collections;
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

	private GameDate m_nextPurchased;
	
	private bool m_firstRun = true;
	private bool m_isAvailable;
	
	#region mono
	private void Start() {
		if (isTestScene) {
			Init(skillCountPerDraw);
		} else {
			UIManager.Instance.onPortalClicked += OnPortalClicked;
		}
	}

	private void OnDestroy() {
		StopCoroutine("ProcessAvailability");
		if (UIManager.Instance != null) {
			UIManager.Instance.onPortalClicked -= OnPortalClicked;
		}
		m_skillItems.ForEach((eachItem) => eachItem.onButtonClick -= OnSkillClick);
	}
	#endregion

	IEnumerator ProcessAvailability() {
		while (m_purchaseSkillUIView.UIModel.parentDisplay.gameObject.activeSelf) {
			m_isAvailable = GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased) <= 0;
			if (!m_isAvailable) {
				m_purchaseSkillUIView.SetMessage("New Abilities will be available after " + (GameManager.Instance.Today().GetTickDifferenceNonAbsoluteOrZeroIfReached(m_nextPurchased)) + " ticks");
			} else {
				DisplayMenu();
			}
			yield return 0;
		}
	}
	public void Init(int numberOfSkills) {
		m_numberOfSkills = numberOfSkills;
		InstantiateUI();
	}

	private void OnPortalClicked() {
		if (GameManager.Instance.gameHasStarted) {
			Init(skillCountPerDraw);
		}
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
		} else {
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			m_purchaseSkillUIView.SetMessage("All Skills Unlocked");
		}
	}

	public void MakeListForAvailableSkills() {
		if (m_weightedList == null) {
			m_weightedList = new WeightedDictionary<SkillData>();
		}
		m_weightedList.Clear();
		foreach (KeyValuePair<PLAYER_SKILL_TYPE, SkillData> entry in PlayerSkillManager.Instance.allPlayerSkillsData) {
			if (!entry.Value.isInUse) {
				PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(entry.Value.type);
				if (playerSkillData != null) {
					if (m_skillProgressionManager.CheckAndUnlock(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.mana, entry.Value.type) != -1) {
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

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_isAvailable || m_firstRun) {
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
			m_purchaseSkillUIView.HideSkills();
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
		});
	}

	private void DisplayMenu() {
		m_purchaseSkillUIView.ShowSkills();
		SpawnItems();
		if (m_isAvailable) {
			StopCoroutine("ProcessAvailability");
			m_purchaseSkillUIView.EnableRerollButton();
		} else {
			StartCoroutine("ProcessAvailability");
		}
	}

	#region Listeners
	public void OnRerollClicked() {
		m_firstRun = false;
		m_nextPurchased = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
		MakeListForAvailableSkills();
		m_purchaseSkillUIView.DisableRerollButton();
		SpawnItems();
		m_isAvailable = false;
		m_purchaseSkillUIView.HideSkills();
		StartCoroutine("ProcessAvailability");
	}
	public void OnCloseClicked() {
		StopCoroutine("ProcessAvailability");
		HideUI();
	}

	public void OnSkillClick(PLAYER_SKILL_TYPE p_type) {
		int result;
		if (isTestScene) {
			result = m_skillProgressionManager.CheckAndUnlock(fakePlayer.skillComponent, fakePlayer.currenciesComponent, p_type);
		} else {
			result = m_skillProgressionManager.CheckAndUnlock(PlayerManager.Instance.player.playerSkillComponent, PlayerManager.Instance.player.mana, p_type);
		}
		if (result != -1) {
			MakeListForAvailableSkills();
			m_firstRun = false;
			m_nextPurchased = GameManager.Instance.Today().AddDays(1);
			PlayerManager.Instance.player.AdjustMana(-result);
			PlayerManager.Instance.player.playerSkillComponent.SetPlayerSkillData(p_type);
			m_purchaseSkillUIView.HideSkills();
			m_purchaseSkillUIView.DisableRerollButton();
			m_isAvailable = false;
			StartCoroutine("ProcessAvailability");
		}
	}
	#endregion
}