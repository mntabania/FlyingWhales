using System;
using UnityEngine;

[Serializable]
public struct TickRange {
    [SerializeField] private int m_startTick;
    [SerializeField] private int m_endTick;
    
    /// <summary>
    /// Helper struct for checking tick ranges.
    /// </summary>
    /// <param name="p_startTick">The tick the range starts at. [inclusive]</param>
    /// <param name="p_endTick">The tick the range ends at. [exclusive]</param>
    public TickRange(int p_startTick, int p_endTick) {
        m_startTick = p_startTick;
        m_endTick = p_endTick;
    }

    public void IncreaseEndTick(int p_amount) {
        for (int i = 0; i < p_amount; i++) {
            m_endTick++;
            if (m_endTick > GameManager.ticksPerDay) {
                m_endTick = 0;
            }
        }
    }
    
    /// <summary>
    /// Is the provided tick within this range?
    /// This handles if a time range is between 2 dates.
    /// Example: if time range is in between 10 PM - 5 AM and provided tick is 11 PM, this will return true.
    /// </summary>
    /// <param name="p_tick">The tick to check.</param>
    public bool IsInRange(int p_tick) {
        if (m_startTick < m_endTick) {
            return p_tick >= m_startTick && p_tick < m_endTick;
        } else if (m_startTick > m_endTick) {
            return p_tick >= m_startTick || p_tick < m_endTick;
        } else {
            //start and end tick are the same. Just check for equality with provided tick
            return p_tick == m_startTick;
        }
    }
    public int GetStartTick() {
        return m_startTick;
    }
    public int GetEndTick() {
        return m_endTick;
    }
    public override string ToString() {
        if (GameManager.Instance != null) {
            return $"{GameManager.Instance.ConvertTickToTime(m_startTick)} - {GameManager.Instance.ConvertTickToTime(m_endTick)}";
        } else {
            return $"{m_startTick.ToString()} - {m_endTick.ToString()}";
        }
    }
}
