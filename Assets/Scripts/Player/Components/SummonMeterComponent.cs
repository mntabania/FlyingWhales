using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class SummonMeterComponent {
    private bool m_isInitialized = false;
    // private int m_currentPoints;
    public static int MAX_TARGET_POINT = 100000; //also the max points

    public RuinarchBasicProgress progress { get; private set; }

    public SummonMeterComponent() {
        progress = new RuinarchBasicProgress("The Ruinarch's Awakening", BOOKMARK_TYPE.Progress_Bar);
        progress.Initialize(0, MAX_TARGET_POINT);
    }
    public SummonMeterComponent(SaveDataSummonMeterComponent data) {
        progress = data.progress;
        progress.Load();
    }
    
    public void Initialize() {
        m_isInitialized = true;
        ListenToSignals();
    }
    private void InitializeOnGameStarted() {
        Messenger.RemoveListener(Signals.GAME_STARTED, InitializeOnGameStarted);
        // progress.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
    }

    public void LoadReferences(SaveDataSummonMeterComponent data) {
        // progress.SetOnSelectAction(() => UIManager.Instance.ShowStructureInfo(PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL)));
    }
    
    void ListenToSignals() {
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyGain);
        Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnChaoticEnergyGain);
        Messenger.AddListener(Signals.GAME_STARTED, InitializeOnGameStarted);
    }

    void OnChaoticEnergyGain(int p_amountGained) {
        progress.IncreaseProgress(p_amountGained);
        // m_currentPoints = Mathf.Clamp(m_currentPoints + p_amountGained, 0, MAX_TARGET_POINT);
        Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, progress.currentValue, MAX_TARGET_POINT);
    }

    void OnSpiritEnergyGain(int p_amountGained, int p_currentSpiritEnergy) {
        progress.IncreaseProgress(p_amountGained);
        // m_currentPoints = Mathf.Clamp(m_currentPoints + (p_amountGained * 5), 0, MAX_TARGET_POINT);
        Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, progress.currentValue, MAX_TARGET_POINT);
    }
}

public class SaveDataSummonMeterComponent : SaveData<SummonMeterComponent> {
    public RuinarchBasicProgress progress;
    public override void Save(SummonMeterComponent data) {
        base.Save(data);
        progress = data.progress;
    }
    public override SummonMeterComponent Load() {
        return new SummonMeterComponent(this);
    }
}