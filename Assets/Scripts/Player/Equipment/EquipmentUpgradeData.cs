using System;
using UnityEngine;
using System.Collections.Generic;
using UtilityScripts;

[System.Serializable]
public class EquipmentUpgradeData {
    [HideInInspector]
    public List<EQUIPMENT_BONUS> bonuses = new List<EQUIPMENT_BONUS>();
    [HideInInspector]
    public float AdditionalPiercing;
    [HideInInspector]
    public float AdditionalmaxHPPercentage;
    [HideInInspector]
    public float AdditionalmaxHPActual;
    [HideInInspector]
    public float AdditionalAttackPercentage;
    [HideInInspector]
    public float AdditionalAttackActual;
    [HideInInspector]
    public float additionalResistanceBonus;
    [HideInInspector]
    public ELEMENTAL_TYPE elementAttackBonus;
    [HideInInspector]
    public EQUIPMENT_SLAYER_BONUS slayerBonus;
    [HideInInspector]
    public EQUIPMENT_WARD_BONUS wardBonus;
}