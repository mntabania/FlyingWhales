using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockStructureUIController : MVCUIController, UnlockStructureUIView.IListener {
    [SerializeField]
    private UnlockStructureUIModel m_unlockStructureUIModel;
    private UnlockStructureUIView m_unlockStructureUIView;
    
    private void OnEnable() {
        UnlockStructureItemUI.onClickUnlockStructure += OnChooseStructureToUnlock;
    }
    private void OnDisable() {
        UnlockStructureItemUI.onClickUnlockStructure -= OnChooseStructureToUnlock;
    }
    public override void ShowUI() {
        m_mvcUIView.ShowUI();
        m_unlockStructureUIView.UpdateStructureItemsSelectableStates();
    }
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI() {
        UnlockStructureUIView.Create(_canvas, m_unlockStructureUIModel, (p_ui) => {
            m_unlockStructureUIView = p_ui;
            m_unlockStructureUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            
            int orderInHierarchy = UIManager.Instance.structureInfoUI.transform.GetSiblingIndex() + 1;
            m_unlockStructureUIView.UIModel.transform.SetSiblingIndex(orderInHierarchy);
            
            Initialize();
        });
    }
    private void Initialize() {
        for (int i = 0; i < PlayerSkillManager.Instance.allDemonicStructureSkills.Length; i++) {
            PLAYER_SKILL_TYPE skillType = PlayerSkillManager.Instance.allDemonicStructureSkills[i];
            UnlockStructureItemUI structureItemUI = m_unlockStructureUIView.UIModel.structureItems[i];
            structureItemUI.SetStructureType(skillType);
            structureItemUI.SetCoverState(false);
        }
        Messenger.AddListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
    }
    private void InitializeAfterLoadoutSelected() {
        Messenger.RemoveListener(UISignals.START_GAME_AFTER_LOADOUT_SELECT, InitializeAfterLoadoutSelected);
        m_unlockStructureUIView.UpdateStructureItemsSelectableStates();
    }
    public void OnClickClose() {
        HideUI();
    }

    private void OnChooseStructureToUnlock(PLAYER_SKILL_TYPE p_structureType, int p_unlockCost) {
        PlayerManager.Instance.player.AdjustMana(-p_unlockCost);
        // PlayerManager.Instance.player.playerSkillComponent.PlayerChoseStructureToUnlock(p_structureType, p_unlockCost);
        HideUI();
    }
}
