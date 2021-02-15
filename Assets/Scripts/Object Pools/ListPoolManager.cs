using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListPoolManager
{
    private static List<List<SkillData>> m_skillDataPool;
    private static bool m_isInitialized = false;

    public static void Initialize() {
        if (!m_isInitialized) {
            ConstructSkillDataListPool();
            m_isInitialized = true;
        }
    }
    #region skillData
    private static void ConstructSkillDataListPool() {
        m_skillDataPool = new List<List<SkillData>>();
    }
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
}
