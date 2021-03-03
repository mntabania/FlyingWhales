using UnityEngine;
using Ruinarch.MVCFramework;
using System.Linq;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class MaraudUIController : MVCUIController, MaraudUIView.IListener {

	[SerializeField]
	private MaraudUIModel m_maraudUIModel;
	private MaraudUIView m_maraudUIView;

	[SerializeField]
	private AvailableMonsterItemUI m_availableMonsterItemUI; //item to instantiate
	private List<AvailableMonsterItemUI> m_summonList = new List<AvailableMonsterItemUI>();
	private List<AvailableMonsterItemUI> m_minionList = new List<AvailableMonsterItemUI>();

	[SerializeField]
	private DeployedMonsterItemUI m_deployedMonsterItemUI; //item to instantiate
	[SerializeField] 
	private List<DeployedMonsterItemUI> m_deployedMonsters = new List<DeployedMonsterItemUI>();

	public FakePlayer fakePlayer;

	private PlayerUnderlingsComponent m_underlingComponent;

	public bool isTestScene;

	private int maxLimitDeployedCount = 3;
	private int currentDeployedCount;

	public int manaCostToDeploySummon = 10;
	public int manaCostToDeployMinion = 100;

	private int m_defaultUnlockedCount = 3;

	private Maraud m_targetMaraudStructure;

	private void Start() {
		if (isTestScene) {
			CharacterManager.Instance.Initialize();
			fakePlayer.Initialize();
			m_underlingComponent = fakePlayer.underlingsComponent;
			Init();
		} else {
			UIManager.Instance.onMaraudClicked += OnMaraudClicked;
		}
	}

	private void OnDestroy() {
		if (UIManager.Instance != null) {
			UIManager.Instance.onMaraudClicked -= OnMaraudClicked;
		}
		m_summonList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
			eachItem.gameObject.SetActive(false);

		});
	}

	private void OnMaraudClicked(LocationStructure p_clickedMaraud) {
		if (GameManager.Instance.gameHasStarted) {
			m_targetMaraudStructure = p_clickedMaraud as Maraud;
			m_underlingComponent = PlayerManager.Instance.player.underlingsComponent;
			Init();
		}
	}

	public void Init() {
		InstantiateUI();
		InitializeMinions();
		InitializeSummons();
	}

	void InitializeMinions() {
		List<PLAYER_SKILL_TYPE> leaders = PlayerSkillManager.Instance.allMinionPlayerSkills.ToList(); //leaders are minion skills
		leaders.ForEach((eachMinsionSkill) => {
			//AvailableMonsterItemUI leaderItem = Instantiate(m_availableMonsterItemUI) as AvailableMonsterItemUI;
			//leaderItem.InitializeItem();
		});
	}

	void InitializeSummons() {
		m_summonList.ForEach((eachItem) => {
			eachItem.onClicked -= OnAvailableMonsterClicked;
			eachItem.gameObject.SetActive(false);

		});
		int ctr = 0;
		foreach (KeyValuePair<SUMMON_TYPE, MonsterUnderlingCharges> entry in m_underlingComponent.monsterUnderlingCharges) {
			// do something with entry.Value or entry.Key
			Debug.LogError(entry.Key + " -- " + CharacterManager.Instance.GetSummonSettings(entry.Key).className);
			SummonSettings settings = CharacterManager.Instance.GetSummonSettings(entry.Key);
			CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(settings.className);
			
			Debug.LogError(cClass.className);
			if (ctr < m_summonList.Count) {
				int currentCharges = 0;
				m_summonList[ctr].gameObject.SetActive(true);
				m_summonList[ctr].isMinion = false;
				m_summonList[ctr].onClicked += OnAvailableMonsterClicked;
				m_targetMaraudStructure.deployedClass.ForEach((eachClass) => {
					if (cClass == eachClass) {
						currentCharges++;
					}
				});
				m_summonList[ctr++].InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges - currentCharges, entry.Value.currentCharges);
				
			} else {
				int currentCharges = 0;
				AvailableMonsterItemUI summonItem = Instantiate(m_availableMonsterItemUI) as AvailableMonsterItemUI;
				m_targetMaraudStructure.deployedClass.ForEach((eachClass) => {
					if (cClass == eachClass) {
						currentCharges++;
					}
				});
				summonItem.InitializeItem(cClass, settings, entry.Key, manaCostToDeploySummon, entry.Value.currentCharges - currentCharges, entry.Value.currentCharges);
				summonItem.isMinion = false;
				summonItem.transform.SetParent(m_maraudUIView.GetAvailableSummonsParent());
				m_summonList.Add(summonItem);
				m_summonList[ctr].onClicked += OnAvailableMonsterClicked;
			}
		}
	}

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI() {
		if (m_maraudUIView == null) {
			MaraudUIView.Create(_canvas, m_maraudUIModel, (p_ui) => {
				m_maraudUIView = p_ui;
				m_maraudUIView.Subscribe(this);
				InitUI(p_ui.UIModel, p_ui);
				m_deployedMonsters = p_ui.UIModel.deployedMonsterItemUIs;
				ShowUI();
			});
		} else {
			ShowUI();
		}
	}

	void OnAvailableMonsterClicked(AvailableMonsterItemUI p_clickedItem) {
		if (!p_clickedItem.isMinion && currentDeployedCount < maxLimitDeployedCount) {
			int price = p_clickedItem.isMinion ? manaCostToDeployMinion : manaCostToDeploySummon;
			p_clickedItem.DeductOneCharge(PlayerManager.Instance.player.mana < price);
			currentDeployedCount++;

			for (int x = 0; x < m_deployedMonsters.Count; ++x) {
				if (!m_deployedMonsters[x].isReadyForDeploy) {
					m_deployedMonsters[x].InitializeItem(p_clickedItem.characterClass, p_clickedItem.summonSettings, p_clickedItem.summonType);
					break;
				}
			}
		}
	}

	void OnDeployedMonsterClicked(DeployedMonsterItemUI p_itemUI) {
		p_itemUI.MakeSlotEmpty();
		currentDeployedCount--;
		m_summonList.ForEach((eachSummon) => {
			if (eachSummon.characterClass == p_itemUI.characterClass) {
				int price = eachSummon.isMinion ? manaCostToDeployMinion : manaCostToDeploySummon;
				eachSummon.AddOneCharge(PlayerManager.Instance.player.mana < price);
			}
		});
	}

	void OnUnlockClicked(DeployedMonsterItemUI p_itemUI) {
		maxLimitDeployedCount++;
	}

	#region MaraudUIView implementation
	public void OnDeployClicked() {
		
	}

	public void OnCloseClicked() {
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