using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class TipsUIController : MVCUIController {

    public Dictionary<TIPS, string> tutorialStrings = new Dictionary<TIPS, string>()
       {
           { TIPS.Time_Manager, "TUTORIAL: Time Management" },
           { TIPS.Target_Menu, "TUTORIAL: Target Menu" },
           { TIPS.Base_Building, "TUTORIAL: Base Building" },
           { TIPS.Unlocking_Powers, "TUTORIAL: Unlocking Bonus Powers" },
           { TIPS.Upgrading_Portal, "TUTORIAL: Upgrading the Portal" },
           { TIPS.Chaotic_Energy, "TUTORIAL: Chaotic Energy" }
       };

    List<TIPS> displayedTips = new List<TIPS>();
    List<TIPS> clickedTips = new List<TIPS>();

    private int currentHour = 0;

    [SerializeField] private TipsItemUI m_tipsItemUI;

    [SerializeField] private TipsUIModel m_tipsUIModel;
    private TipsUIView m_tipsUIView;

    public List<TipsItemUI> tipsItems = new List<TipsItemUI>();
    public SaveDataPlayer m_saveDataPlayer;
    private void Start() {
        InstantiateUI();
    }

	//Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
	[ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        TipsUIView.Create(_canvas, m_tipsUIModel, (p_ui) => {
            m_tipsUIView = p_ui;
            InitUI(p_ui.UIModel, p_ui);
            SubscribeToEvents();
            p_ui.UIModel.transform.SetSiblingIndex(siblingIndex);
        });
    }

	private void OnDisable() {
        UnSubscribeToEvents();
    }

	void SubscribeToEvents() {
        Messenger.AddListener(Signals.GAME_STARTED, OnGameStarted);
        Messenger.AddListener(PlayerSignals.CHAOS_ORB_COLLECTED, OnOrbCollected);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyCollected);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructureObjectPlaced);
        Messenger.AddListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, OnContextMenuClicked);
        Messenger.AddListener(Signals.HOUR_STARTED, HourlyCheck);
        Messenger.AddListener(Signals.PROGRESSION_LOADED, OnSavedProgressionLoaded);
    }
    void UnSubscribeToEvents() {
        Messenger.RemoveListener(Signals.GAME_STARTED, OnGameStarted);
        Messenger.RemoveListener(PlayerSignals.CHAOS_ORB_COLLECTED, OnOrbCollected);
        Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyCollected);
        Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructureObjectPlaced);
        Messenger.RemoveListener<IPlayerActionTarget>(UISignals.PLAYER_ACTION_CONTEXT_MENU_SHOWN, OnContextMenuClicked);
        Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyCheck);
        Messenger.RemoveListener(Signals.PROGRESSION_LOADED, OnSavedProgressionLoaded);
        tipsItems.ForEach((eachItem) => eachItem.onClickTip -= OnItemClicked);
    }

    void AddTips(TIPS p_tips) {
        if (displayedTips.Contains(p_tips) || clickedTips.Contains(p_tips)) {
            return;
        }
        TipsItemUI item = Instantiate(m_tipsItemUI);
        item.transform.SetParent(m_tipsUIView.GetContentParent());
        item.SetDescription(tutorialStrings[p_tips]);
        item.tip = p_tips;
        item.onClickTip += OnItemClicked;
        tipsItems.Add(item);
        displayedTips.Add(p_tips);
        item.PlayIntroAnimation();
    }

    void OnItemClicked(TIPS p_tips) {
        switch (p_tips) {
            case TIPS.Time_Manager:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Time_Management);
            break;
            case TIPS.Chaotic_Energy:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Chaotic_Energy);
            break;
            case TIPS.Base_Building:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Base_Building);
            break;
            case TIPS.Unlocking_Powers:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Unlocking_Bonus_Powers);
            break;
            case TIPS.Upgrading_Portal:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Upgrading_The_Portal);
            break;
            case TIPS.Target_Menu:
            PlayerUI.Instance.ShowSpecificTutorial(Tutorial.TutorialManager.Tutorial_Type.Target_Menu);
            break;
        }
        TipsItemUI ti = null;
        tipsItems.ForEach((eachItem) => {
            if (eachItem.tip == p_tips) {
                ti = eachItem;
            }
        });
        tipsItems.Remove(ti);
        Destroy(ti.gameObject);
        clickedTips.Add(p_tips);
        m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
        m_saveDataPlayer.unlockedTips.Add(p_tips);
        SaveManager.Instance.savePlayerManager.SavePlayerData();
    }
    #region TipsUIView.IListener Implementation
    public void OnClickClose() {
        HideUI();
    }
    #endregion

    #region events listeners
    private void OnContextMenuClicked(IPlayerActionTarget p_action) {
        AddTips(TIPS.Target_Menu);
    }
    private void HourlyCheck() {
        currentHour++;
        switch (currentHour) {
            case 1: AddTips(TIPS.Chaotic_Energy);
            break;
            case 2: AddTips(TIPS.Target_Menu);
            break;
            case 3: AddTips(TIPS.Base_Building);
            break;
            case 4: AddTips(TIPS.Unlocking_Powers);
            break;
        }
    }
    private void OnGameStarted() {
        m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
        m_saveDataPlayer.unlockedTips.ForEach((eachTip) => clickedTips.Add(eachTip));
        
        AddTips(TIPS.Time_Manager);
    }
    private void OnSavedProgressionLoaded() {
        m_saveDataPlayer = SaveManager.Instance.currentSaveDataPlayer;
        m_saveDataPlayer.unlockedTips.ForEach((eachTip) => clickedTips.Add(eachTip));
    }

    private void OnSpiritEnergyCollected(int adjustedAmount, int spiritEnergy) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement != null) {
            ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
            if (!portal.IsMaxLevel()) {
                PortalUpgradeTier nextTier = portal.nextTier;
                if (spiritEnergy >= nextTier.upgradeCost[0].amount) {
                    AddTips(TIPS.Upgrading_Portal);
                }    
            }
        }
    }

    private void OnOrbCollected() {
        AddTips(TIPS.Chaotic_Energy);
    }

    private void OnStructureObjectPlaced(LocationStructure p_structure) {
        if (GameManager.Instance.gameHasStarted) {
            if (p_structure is DemonicStructure) {
                AddTips(TIPS.Base_Building);
            }
        }
    }
    #endregion
}