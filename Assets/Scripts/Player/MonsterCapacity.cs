using System;
using UnityEngine;

[System.Serializable]
public struct MonsterCapacity {
    [SerializeField] private SUMMON_TYPE m_summonType;
    [SerializeField] private RACE m_monsterRace;
    [SerializeField] private string m_strMonsterClass;
    [SerializeField] private readonly string m_strSummonType;
    [SerializeField] private int m_remainingCharges;
    [SerializeField] private int m_maxCapacity;

    #region getters
    public int remainingCharges => m_remainingCharges;
    public int maxCapacity => m_maxCapacity;
    public string strSummonType => m_strSummonType;
    public RACE monsterRace => m_monsterRace;
    public string strMonsterClass => m_strMonsterClass;
    public SUMMON_TYPE summonType => m_summonType;
    #endregion

    public MonsterCapacity(SUMMON_TYPE p_summonType, RACE p_monsterRace, string p_monsterClass) {
        m_summonType = p_summonType;
        m_monsterRace = p_monsterRace;
        m_strMonsterClass = p_monsterClass;
        m_strSummonType = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_summonType.ToString());
        m_remainingCharges = 0;
        m_maxCapacity = 0;
    }

    public void AdjustRemainingCharges(int p_adjustment) {
        m_remainingCharges += p_adjustment;
    }
    public void AdjustMaxCapacity(int p_adjustment) {
        m_maxCapacity += p_adjustment;
    }
}