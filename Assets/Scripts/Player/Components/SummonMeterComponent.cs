using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonMeterComponent
{
    private bool m_isInitialized = false;
    private int m_currentPoints;
    public static int MAX_TARGET_POINT = 10000; //also the max points

    public void Initialize() {
        m_isInitialized = true;
        ListenToSignals();
    }

    void ListenToSignals() {
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyGain);
        Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnChaoticEnergyGain);
    }

    void OnChaoticEnergyGain(int p_amountGained) {
        m_currentPoints = Mathf.Clamp(m_currentPoints + p_amountGained, 0, MAX_TARGET_POINT);
        Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, m_currentPoints, MAX_TARGET_POINT);
    }

    void OnSpiritEnergyGain(int p_amountGained, int p_currentSpiritEnergy) {
        m_currentPoints = Mathf.Clamp(m_currentPoints + (p_amountGained * 5), 0, MAX_TARGET_POINT);
        Messenger.Broadcast(PlayerSignals.PLAYER_SUMMON_METER_UPDATE, m_currentPoints, MAX_TARGET_POINT);
    }
}
