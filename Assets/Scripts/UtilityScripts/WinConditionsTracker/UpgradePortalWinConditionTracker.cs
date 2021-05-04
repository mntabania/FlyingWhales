using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class UpgradePortalWinConditionTracker : WinConditionTracker {
    private const int TargetLevel = 8;
    public override Type serializedData => typeof(SaveDataUpgradePortalWinConditionTracker);

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
        Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
    }
    private void OnPortalUpgraded(int p_newLevel) {
        UpdateStepsChangedNameEvent();
        if (p_newLevel >= TargetLevel) {
            Messenger.Broadcast(PlayerSignals.WIN_GAME, "The almighty demon has been summoned. Congratulations!");
        }
    }

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable upgradePortal = new GenericTextBookmarkable(GetUpgradePortalText, () => BOOKMARK_TYPE.Text, OnSelectWinCondition, 
            null, OnHoverOverAction, OnHoverOutAction);
        IBookmarkable[] bookmarkables = new[] {
            upgradePortal
        };
        return bookmarkables;
    }
    private string GetUpgradePortalText() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        return $"Upgrade Portal to Max {portal.level}/{TargetLevel.ToString()}";
    }
    private void OnSelectWinCondition() {
        ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
        UIManager.Instance.ShowUpgradePortalUI(portal);
    }
    private void OnHoverOverAction(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo("Gather enough Spirit Energy by producing Chaos Orbs and then spend them to upgrade your Portal to unlock new Powers. " +
                                         "You win the game if you upgrade your Portal to Level 8.", pos: position);
    }
    private void OnHoverOutAction() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}

public class SaveDataUpgradePortalWinConditionTracker : SaveDataWinConditionTracker {
    public override void Save(WinConditionTracker data) {
        base.Save(data);
    }
}