using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RequirementData {

    [HideInInspector]
    public List<UNLOCKING_SKILL_REQUIREMENT> requirements = new List<UNLOCKING_SKILL_REQUIREMENT>();

    [HideInInspector]
    public PLAYER_ARCHETYPE requiredArchetype;
    [HideInInspector]
    public List<PLAYER_SKILL_TYPE> requiredSkills;
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
    public bool isOR;
}
