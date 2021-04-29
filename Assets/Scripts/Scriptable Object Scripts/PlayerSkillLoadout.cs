using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

using UtilityScripts;

[CreateAssetMenu(fileName = "New Player Skill Loadout", menuName = "Scriptable Objects/Player Skill Loadout")]
public class PlayerSkillLoadout : ScriptableObject {

    public const int MAX_SKILLS_PER_UPGRADE_TIER = 8;
    
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
        // if (archetype.IsScenarioArchetype()) {
        //     if (availableStructures.Contains(PLAYER_SKILL_TYPE.SPIRE) || structures.fixedSkills.Contains(PLAYER_SKILL_TYPE.SPIRE)) {
        //         Debug.LogError($"{this.name} should not have Spire available in loadout! Since it is a scenario loadout. Reference: https://trello.com/c/PVEd5Ti8/3993-fixed-loadouts-for-custom-scenarios");
        //     }
        // }
        if (portalUpgradeTiers != null) {
            if (portalUpgradeTiers.Length == 0) {
                Debug.LogError($"{this.name} has no portal tiers! All Archetypes should have at least 1!!");
                return;
            }
            for (int i = 0; i < portalUpgradeTiers.Length; i++) {
                PortalUpgradeTier tier = portalUpgradeTiers[i];
                tier.name = $"Level {(i + 1).ToString()}";
                for (int j = 0; j < tier.upgradeCost.Length; j++) {
                    Cost cost = tier.upgradeCost[j];
                    cost.name = cost.ToString();
                    tier.upgradeCost[j] = cost;
                }
                portalUpgradeTiers[i] = tier;

                // int passiveSkillsLength = tier.passiveSkillsToUnlock?.Length ?? 0;
                int skillTypesLength = tier.skillTypesToUnlock?.Length ?? 0;
                int totalThingsToUnlockInTier = skillTypesLength;
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
    // public PASSIVE_SKILL[] passiveSkillsToUnlock;
    public Cost[] upgradeCost;
    public int upgradeTime;

    public string GetUpgradeCostString() {
        string combined = string.Empty;
        for (int i = 0; i < upgradeCost.Length; i++) {
            Cost cost = upgradeCost[i];
            string costSprite = cost.currency.GetCurrencyTextSprite();
            combined = $"{combined} {cost.processedAmount.ToString()}{costSprite}";
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

    public int processedAmount => SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());

    public Cost(CURRENCY p_currency, int p_amount) {
        currency = p_currency;
        amount = p_amount;
        name = $"{SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification()).ToString()} {currency.ToString()}";
    }
    
    public override string ToString() {
        if (WorldSettings.Instance != null) {
            return $"{processedAmount.ToString()} {currency.ToString()}";
        }
        return $"{amount.ToString()} {currency.ToString()}";
    }
    public string GetCostStringWithIcon() {
        string icon = string.Empty;
        switch (currency) {
            case CURRENCY.Mana:
                icon = UtilityScripts.Utilities.ManaIcon();
                break;
            case CURRENCY.Chaotic_Energy:
                icon = UtilityScripts.Utilities.ChaoticEnergyIcon();
                break;
            case CURRENCY.Spirit_Energy:
                icon = UtilityScripts.Utilities.SpiritEnergyIcon();
                break;
            default:
                icon = string.Empty;
                break;
        }
        return $"{processedAmount.ToString()}{icon}";
    }
}