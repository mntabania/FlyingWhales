using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this data holds stats bonus updates everytime villager's skill is leveled up. dont apply to elves(for now)
[System.Serializable]
public class CharacterSkillUpdateData
{
    public float piercingBonus;
    public float mentalResistanceBonus;
    public float physicalResistanceBonus;
    public float normalResistanceBonus;
    public float fireResistanceBonus;
    public float poisonResistanceBonus;
    public float waterResistanceBonus;
    public float iceResistanceBonus;
    public float electricResistanceBonus;
    public float earthResistanceBonus;
    public float windResistanceBonus;
    public float allElementresistanceBonus;

    public float GetBonusBaseOnElement(ELEMENTAL_TYPE p_bonusForThisElement) {
		switch (p_bonusForThisElement) {
            case ELEMENTAL_TYPE.Normal:   return normalResistanceBonus;
            case ELEMENTAL_TYPE.Fire: return fireResistanceBonus;
            case ELEMENTAL_TYPE.Poison: return poisonResistanceBonus;
            case ELEMENTAL_TYPE.Water: return waterResistanceBonus;
            case ELEMENTAL_TYPE.Ice: return iceResistanceBonus;
            case ELEMENTAL_TYPE.Electric: return electricResistanceBonus;
            case ELEMENTAL_TYPE.Earth: return earthResistanceBonus;
            case ELEMENTAL_TYPE.Wind: return windResistanceBonus;
        }
        return 0;
	}

    public float GetPhysicalResistanceBonus() {
        return physicalResistanceBonus;
    }

    public float GetMentalResistanceBonus() {
        return mentalResistanceBonus;
    }

    public float GetAllElementResistanceBonus() {
        return allElementresistanceBonus;
    }
}   
