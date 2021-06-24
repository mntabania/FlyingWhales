using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this data holds stats bonus updates everytime villager's skill is leveled up. dont apply to elves(for now)
[System.Serializable]
public class CharacterProgressionBonusData
{
    [Space]
    [Header("Elemental Resistance")]
    [Space]
    [SerializeField] private float m_fireResistanceBonus;
    [SerializeField] private float m_earthResistanceBonus;
    [SerializeField] private float m_windResistanceBonus;
    [SerializeField] private float m_waterResistanceBonus;
    [Space]
    [SerializeField] private float m_allElementalresistanceBonus;
    [Space]
    [Header("Secondary Resistance")]
    [Space]
    [SerializeField] private float m_poisonResistanceBonus;
    [SerializeField] private float m_iceResistanceBonus;
    [SerializeField] private float m_electricResistanceBonus;
    [Space]
    [SerializeField] private float m_allSecondaryresistanceBonus;
    [Space]
    [Space]
    [Header("Other")]
    //[SerializeField] private float m_normalResistanceBonus;
    [SerializeField] private float m_piercingBonus;
    [SerializeField] private float m_mentalResistanceBonus;
    [SerializeField] private float m_physicalResistanceBonus;

    public float GetBonusBaseOnElement(RESISTANCE p_bonusForThisElement) {
		switch (p_bonusForThisElement) {
            //case RESISTANCE.Normal: return m_normalResistanceBonus;
            case RESISTANCE.Fire: return m_fireResistanceBonus;
            case RESISTANCE.Poison: return m_poisonResistanceBonus;
            case RESISTANCE.Water: return m_waterResistanceBonus;
            case RESISTANCE.Ice: return m_iceResistanceBonus;
            case RESISTANCE.Electric: return m_electricResistanceBonus;
            case RESISTANCE.Earth: return m_earthResistanceBonus;
            case RESISTANCE.Wind: return m_windResistanceBonus;
            case RESISTANCE.Physical: return m_physicalResistanceBonus;
            case RESISTANCE.Mental: return m_mentalResistanceBonus;
        }
        return 0;
	}

    public float GetPiercingBonus() {
        return m_piercingBonus;
    }

    public float GetPhysicalResistanceBonus() {
        return m_physicalResistanceBonus;
    }

    public float GetMentalResistanceBonus() {
        return m_mentalResistanceBonus;
    }

    public float GetAllElementalResistanceBonus() {
        return m_allElementalresistanceBonus;
    }

    public float GetAllSecondaryResistanceBonus() {
        return m_allSecondaryresistanceBonus;
    }
}   
