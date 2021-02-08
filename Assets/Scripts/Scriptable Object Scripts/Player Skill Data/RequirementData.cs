using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RequirementData {
    public List<SkillData> requiredSkills;
    public int actionCount;
    public int afflictionCount;
    public int spellsCount;
    public int tier1Count;
    public int tier2Count;
    public int tier3Count;
}
