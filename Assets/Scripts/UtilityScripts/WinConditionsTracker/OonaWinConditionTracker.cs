using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class OonaWinConditionTracker : WinConditionTracker {

    private System.Action<int, int> _OnSummonMeterUpdated;
    private System.Action<int> _OnPortalLevelUpgraded;

    public interface ListenerPortalUpgrade {
        void OnPortalLevelUpgraded(int p_newLevel);
    }

    public int currentLevel;
    public int targetLevel;
    public override Type serializedData => typeof(SaveDataOonaWinConditionTracker);

    public override void Initialize(List<Character> p_allCharacters) {
        targetLevel = EditableValuesManager.Instance.GetTargetPortalLevel();
        base.Initialize(p_allCharacters);
        Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPortalUpgraded);
    }
    protected override IBookmarkable[] CreateWinConditionSteps() { return null; }
    public void OnPortalUpgraded(int p_newLevel) {
        _OnPortalLevelUpgraded?.Invoke(p_newLevel);
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataOonaWinConditionTracker tracker = data as SaveDataOonaWinConditionTracker;
        currentLevel = tracker.currentLevel;
        targetLevel = tracker.targetLevel;
    }
    #endregion

    public void SubscribeToPortalUpgraded(OonaWinConditionTracker.ListenerPortalUpgrade p_listener) {
        _OnPortalLevelUpgraded += p_listener.OnPortalLevelUpgraded;
    }
    public void UnsubscribeToPortalUpgraded(OonaWinConditionTracker.ListenerPortalUpgrade p_listener) {
        _OnPortalLevelUpgraded -= p_listener.OnPortalLevelUpgraded;
    }
}

public class SaveDataOonaWinConditionTracker : SaveDataWinConditionTracker {
    public int currentLevel;
    public int targetLevel;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        OonaWinConditionTracker tracker = data as OonaWinConditionTracker;
        currentLevel = tracker.currentLevel;
        targetLevel = tracker.targetLevel;
    }
}