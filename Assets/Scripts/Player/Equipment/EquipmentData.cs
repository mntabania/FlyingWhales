using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Scriptable Objects/Player Skills/EquipmentData")]
public class EquipmentData : ScriptableObject {
    public RESOURCE resourceType;
    public int resourceAmount;
    public int purchaseCost;
    public int tier;
    public string description;
    public Sprite imgIcon;

    public EquipmentUpgradeData equipmentUpgradeData;
}

//[System.Serializable]
//public class PlayerSkillDataCopy {
//    public SPELL_TYPE skill;
//    public int manaCost;
//    public int charges;
//    public int cooldown;
//    public int threat;
//    public int threatPerHour;
//}