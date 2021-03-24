﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[CreateAssetMenu(fileName = "New Player Skill Loadout", menuName = "Scriptable Objects/Player Skill Loadout")]
public class PlayerSkillLoadout : ScriptableObject {

    public const int MAX_SKILLS_PER_UPGRADE_TIER = 6;
    
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
        if (portalUpgradeTiers != null) {
            for (int i = 0; i < portalUpgradeTiers.Length; i++) {
                PortalUpgradeTier tier = portalUpgradeTiers[i];
                tier.name = $"Level {(i + 1).ToString()}";
                for (int j = 0; j < tier.upgradeCost.Length; j++) {
                    Cost cost = tier.upgradeCost[j];
                    cost.name = cost.ToString();
                    tier.upgradeCost[j] = cost;
                }
                portalUpgradeTiers[i] = tier;

                int passiveSkillsLength = tier.passiveSkillsToUnlock?.Length ?? 0;
                int skillTypesLength = tier.skillTypesToUnlock?.Length ?? 0;
                int totalThingsToUnlockInTier = passiveSkillsLength + skillTypesLength;
                if (totalThingsToUnlockInTier > MAX_SKILLS_PER_UPGRADE_TIER) {
                    Debug.LogError($"{this.name} ({tier.name}) has {totalThingsToUnlockInTier.ToString()} upgrades but maximum amount should only be {MAX_SKILLS_PER_UPGRADE_TIER.ToString()}!");
                }
            }    
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
    public int upgradeTime;

    public string GetUpgradeCostString() {
        string combined = string.Empty;
        for (int i = 0; i < upgradeCost.Length; i++) {
            Cost cost = upgradeCost[i];
            string costSprite = cost.currency.GetCurrencyTextSprite();
            combined = $"{combined} {cost.amount.ToString()}{costSprite}";
            if (!upgradeCost.IsLastIndex(i)) {
                combined = $"{combined},";
            }
        }
        return combined;
    }
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