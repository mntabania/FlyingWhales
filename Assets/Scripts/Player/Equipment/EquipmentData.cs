using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UtilityScripts;

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Scriptable Objects/Equipments/EquipmentData")]
public class EquipmentData : ScriptableObject {
    [HideInInspector]
    [Header("if filled = any of type")]
    [Tooltip("if filled ignore specific resources")]
    public RESOURCE resourceType = RESOURCE.NONE;
    [HideInInspector]
    [Tooltip("if resources is not NONE, this one will be ignored")]
    public CONCRETE_RESOURCES specificResource;
    public int resourceAmount;
    public int purchaseCost;
    public int tier;
    public List<EQUIPMENT_CLASS_COMPATIBILITY> compatibleClasses = new List<EQUIPMENT_CLASS_COMPATIBILITY>();
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