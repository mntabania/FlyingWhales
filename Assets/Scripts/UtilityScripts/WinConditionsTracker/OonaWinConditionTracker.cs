using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class OonaWinConditionTracker : WinconditionTracker {

    private System.Action<int, int> _OnSummonMeterUpdated;

    public interface Listener {
        void OnSummonMeterUpdated(int p_currentSummonCount, int p_targetSummonCount);
    }

    public int currentSummonPoints;
    public int targetSummonPoints = SummonMeterComponent.MAX_TARGET_POINT;
    public override Type serializedData => typeof(SaveDataOonaWinConditionTracker);

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, OnSummonMeterUpdate);
    }

    public void OnSummonMeterUpdate(int p_currentSummonPoints, int p_targetSummonPoints) {
        _OnSummonMeterUpdated?.Invoke(p_currentSummonPoints, p_targetSummonPoints);
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataOonaWinConditionTracker tracker = data as SaveDataOonaWinConditionTracker;
        currentSummonPoints = tracker.currentSummonPoints;
        targetSummonPoints = tracker.targetSummonPoints;
    }
    #endregion

    public void Subscribe(OonaWinConditionTracker.Listener p_listener) {
        _OnSummonMeterUpdated += p_listener.OnSummonMeterUpdated;
    }
    public void Unsubscribe(OonaWinConditionTracker.Listener p_listener) {
        _OnSummonMeterUpdated -= p_listener.OnSummonMeterUpdated;
    }
}

public class SaveDataOonaWinConditionTracker : SaveDataWinConditionTracker {
    public int currentSummonPoints;
    public int targetSummonPoints;
    public override void Save(WinconditionTracker data) {
        base.Save(data);
        OonaWinConditionTracker tracker = data as OonaWinConditionTracker;
        currentSummonPoints = tracker.currentSummonPoints;
        targetSummonPoints = tracker.targetSummonPoints;
    }
}