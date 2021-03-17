using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonMeterComponent
{
    private bool m_isInitialized = false;
    private int m_currentPoints;
    private int m_targetPoints = 10000; //also the max points

    public void Initialize() {
        m_isInitialized = true;
        ListenToSignals();
    }

    void ListenToSignals() {
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyGain);
        Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, OnChaoticEnergyGain);
    }

    void OnChaoticEnergyGain(int p_amountGained) {
        m_currentPoints = Mathf.Clamp(m_currentPoints + p_amountGained, 0, m_targetPoints);
    }

    void OnSpiritEnergyGain(int p_amountGained, int p_currentSpiritEnergy) {
        m_currentPoints = Mathf.Clamp(m_currentPoints + (p_amountGained * 5), 0, m_targetPoints);
    }
}
