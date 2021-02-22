using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListPoolManager
{
    private static List<List<SkillData>> m_skillDataPool;
    private static List<List<PLAYER_SKILL_TYPE>> m_skillTypePool;
    private static bool m_isInitialized = false;

    public static void Initialize() {
        if (!m_isInitialized) {
            m_skillDataPool = new List<List<SkillData>>();
            m_skillTypePool = new List<List<PLAYER_SKILL_TYPE>>();
            m_isInitialized = true;
        }
    }
    
    #region skillData
    public static List<SkillData> CreateNewSkillDataList() {
        if (m_skillDataPool.Count > 0) {
            List<SkillData> data = m_skillDataPool[0];
            m_skillDataPool.RemoveAt(0);
            return data;
        }
        return new List<SkillData>();
    }
    public static void ReturnSkillDataListToPool(List<SkillData> data) {
        data.Clear();
        m_skillDataPool.Add(data);
    }
    #endregion

    #region PLAYER_SKILL_TYPE
    public static List<PLAYER_SKILL_TYPE> CreateNewPlayerSkillTypeList() {
        if (m_skillTypePool.Count > 0) {
            List<PLAYER_SKILL_TYPE> data = m_skillTypePool[0];
            m_skillTypePool.RemoveAt(0);
            return data;
        }
        return new List<PLAYER_SKILL_TYPE>();
    }
    public static void ReturnPlayerSkillTypeListToPool(List<PLAYER_SKILL_TYPE> data) {
        data.Clear();
        m_skillTypePool.Add(data);
    }
    #endregion
}
