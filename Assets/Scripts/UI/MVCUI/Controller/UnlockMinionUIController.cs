using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockMinionUIController : MVCUIController, UnlockMinionUIView.IListener {
    [SerializeField]
    private UnlockMinionUIModel m_unlockMinionUIModel;
    private UnlockMinionUIView m_unlockMinionUIView;
    
    private void OnEnable() {
        UnlockMinionItemUI.onClickUnlockMinion += OnChooseMinionToUnlock;
    }
    private void OnDisable() {
        UnlockMinionItemUI.onClickUnlockMinion -= OnChooseMinionToUnlock;
    }
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        UnlockMinionUIView.Create(_canvas, m_unlockMinionUIModel, (p_ui) => {
            m_unlockMinionUIView = p_ui;
            m_unlockMinionUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            
            int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
            m_unlockMinionUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);
            
            Initialize();
        });
    }
    private void Initialize() {
        for (int i = 0; i < PlayerSkillManager.Instance.allMinionPlayerSkills.Length; i++) {
            PLAYER_SKILL_TYPE skillType = PlayerSkillManager.Instance.allMinionPlayerSkills[i];
            UnlockMinionItemUI minionItemUI = m_unlockMinionUIView.UIModel.minionItems[i];
            minionItemUI.SetMinionType(skillType);
            minionItemUI.SetCoverState(false);
            minionItemUI.SetCheckmarkState(false);
        }
        Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
    }
    private void InitializeAfterLoadoutSelected() {
        Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
        m_unlockMinionUIView.UpdateMinionItemsSelectableStates();
    }
    public override void ShowUI() {
        m_mvcUIView.ShowUI();
        m_unlockMinionUIView.UpdateMinionItemsSelectableStates();
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
    }
    public override void HideUI() {
        base.HideUI();
        UIManager.Instance.SetSpeedTogglesState(true);
        UIManager.Instance.ResumeLastProgressionSpeed();
    }
    public void OnClickClose() {
        HideUI();
    }
    private void OnChooseMinionToUnlock(PLAYER_SKILL_TYPE p_minionType, int p_unlockCost) {
        PlayerManager.Instance.player.AdjustMana(-p_unlockCost);
        // PlayerManager.Instance.player.playerSkillComponent.PlayerStartedPortalUpgrade(p_minionType, p_unlockCost);
        HideUI();
    }
}
