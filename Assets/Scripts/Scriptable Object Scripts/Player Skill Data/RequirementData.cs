using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RequirementData {

    [HideInInspector]
    public List<UNLOCKING_SKILL_REQUIREMENT> requirements = new List<UNLOCKING_SKILL_REQUIREMENT>();

    [HideInInspector]
    public List<PLAYER_ARCHETYPE> requiredArchetypes = new List<PLAYER_ARCHETYPE>();
    public List<PLAYER_SKILL_TYPE> requiredSkills = new List<PLAYER_SKILL_TYPE>();
    [HideInInspector]
    public int actionCount;
    [HideInInspector]
    public int afflictionCount;
    [HideInInspector]
    public int spellsCount;
    [HideInInspector]
    public int tier1Count;
    [HideInInspector]
    public int tier2Count;
    [HideInInspector]
    public int tier3Count;
    [HideInInspector]
    public int portalLevel;
    [HideInInspector]
    public bool isOR;
}
