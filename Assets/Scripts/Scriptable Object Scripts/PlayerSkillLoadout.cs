using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[CreateAssetMenu(fileName = "New Player Skill Loadout", menuName = "Scriptable Objects/Player Skill Loadout")]
public class PlayerSkillLoadout : ScriptableObject {
    public PLAYER_ARCHETYPE archetype;
    public PlayerSkillLoadoutData spells;
    public PlayerSkillLoadoutData afflictions;
    public PlayerSkillLoadoutData minions;
    public PlayerSkillLoadoutData structures;
    public PlayerSkillLoadoutData miscs;
    public PASSIVE_SKILL[] passiveSkills;

    public PLAYER_SKILL_TYPE[] availableSpells;
    public PLAYER_SKILL_TYPE[] availableAfflictions;
    public PLAYER_SKILL_TYPE[] availableMinions;
    public PLAYER_SKILL_TYPE[] availableStructures;
    public PLAYER_SKILL_TYPE[] availableMiscs;

    [Header("Portal Upgrades")]
    public PortalUpgradeTier[] portalUpgradeTiers;
    
    private void OnValidate() {
        for (int i = 0; i < portalUpgradeTiers.Length; i++) {
            PortalUpgradeTier tier = portalUpgradeTiers[i];
            tier.name = $"Level {(i + 1).ToString()}";
            for (int j = 0; j < tier.upgradeCost.Length; j++) {
                Cost cost = tier.upgradeCost[j];
                cost.name = cost.ToString();
                tier.upgradeCost[j] = cost;
            }
            portalUpgradeTiers[i] = tier;
        }
    }
}

[System.Serializable]
public class PlayerSkillLoadoutData {
    public int extraSlots;
    public List<PLAYER_SKILL_TYPE> fixedSkills;
}
[System.Serializable]
public struct PortalUpgradeTier {
    [ReadOnly] public string name;
    public PLAYER_SKILL_TYPE[] skillTypesToUnlock;
    public PASSIVE_SKILL[] passiveSkillsToUnlock;
    public Cost[] upgradeCost;
}
[System.Serializable]
public struct Cost {
    [ReadOnly] public string name;
    public CURRENCY currency;
    public int amount;
    public override string ToString() {
        return $"{amount.ToString()} {currency.ToString()}";
    }
}