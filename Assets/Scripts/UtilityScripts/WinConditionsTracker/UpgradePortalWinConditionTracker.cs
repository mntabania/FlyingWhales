using System;
using System.Collections.Generic;

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
        GenericTextBookmarkable upgradePortal = new GenericTextBookmarkable(GetUpgradePortalText, () => BOOKMARK_TYPE.Text, null, null);
        IBookmarkable[] bookmarkables = new[] {
            upgradePortal
        };
        return bookmarkables;
    }
    private string GetUpgradePortalText() {
        return $"Upgrade Portal to Level {TargetLevel.ToString()}";
    }
    #endregion
}

public class SaveDataUpgradePortalWinConditionTracker : SaveDataWinConditionTracker {
    public override void Save(WinConditionTracker data) {
        base.Save(data);
    }
}