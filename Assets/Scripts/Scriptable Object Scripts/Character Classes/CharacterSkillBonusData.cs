using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Villager Skill Bonus Data", menuName = "Scriptable Objects/VIllager Skills/VillagerSkillBonusData")]
public class CharacterSkillBonusData : ScriptableObject {
    public List<SkillBonus> bonusPerLevel = new List<SkillBonus>();
}

[System.Serializable]
public class SkillBonus {
    public int maxHPBonus;
    public int attackBonus;
    public int critBonus;
    public List<string> canBecomeClasses = new List<string>();
}
