using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inner_Maps.Location_Structures;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UtilityScripts;
using Locations.Settlements;
using Debug = System.Diagnostics.Debug;

public class PlayerSkillDetailsTooltip : MonoBehaviour {
    public RectTransform thisRect;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public RuinarchText descriptionText;
    public TextMeshProUGUI currenciesText;
    public TextMeshProUGUI bonusesText;
    public TextMeshProUGUI additionalText;
    public UIHoverPosition defaultPosition;

    public void ShowPlayerSkillDetails(SkillData spellData, UIHoverPosition position = null) {
        UpdateData(spellData);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillDetails(PlayerSkillData skillData, UIHoverPosition position = null) {
        UpdateData(skillData);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillDetails(string title, string description, UIHoverPosition position = null) {
        UpdateData(title, description);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillWithLevelUpDetails(SkillData spellData, UIHoverPosition position = null) {
        UpdateDataWithLevelUpDetails(spellData);
        UpdatePosition(position);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
    }
    private void UpdatePosition(UIHoverPosition position) {
        gameObject.SetActive(true);
        UIHoverPosition positionToUse = position;
        if (positionToUse == null) {
            positionToUse = defaultPosition;
        }

        Debug.Assert(thisRect != null, nameof(thisRect) + " != null");
        thisRect.SetParent(positionToUse.transform);

        thisRect.pivot = positionToUse.pivot;
        UtilityScripts.Utilities.GetAnchorMinMax(positionToUse.anchor, out var anchorMin, out var anchorMax);
        thisRect.anchorMin = anchorMin;
        thisRect.anchorMax = anchorMax;
        thisRect.anchoredPosition = Vector2.zero;
        thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 264f);
    }
    private void UpdateData(PlayerSkillData skillData) {
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skillData.skill);
        titleText.SetText(spellData.name);
        descriptionText.SetTextAndReplaceWithIcons(spellData.description);
        int charges =  SpellUtilities.GetModifiedSpellCost(skillData.charges, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeCostsModification());
        int manaCost = SpellUtilities.GetModifiedSpellCost(skillData.manaCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
        int cooldown = SpellUtilities.GetModifiedSpellCost(skillData.cooldown, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification());
        //int threat = SpellUtilities.GetModifiedSpellCost(skillData.threat, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetThreatModification());

        //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
        string currencyStr = GetCurrencySummary(manaCost, charges, charges, cooldown); 
        
        levelText.text = string.Empty;
        currenciesText.text = currencyStr;
        additionalText.text = string.Empty;
        bonusesText.text = GetBonusesString(skillData, 1);
        if (charges > 0) {
            //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
            bonusesText.text = $"{bonusesText.text}{UtilityScripts.Utilities.ColorizeSpellTitle("Charges:")} {charges.ToString()}/{charges.ToString()}";
        }
    }
    private void UpdateData(SkillData spellData) {
        titleText.text = spellData.name;
        string fullDescription = spellData.description;
        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
        
        string currencyStr = GetCurrencySummary(manaCost, charges, spellData.maxCharges, cooldown);
        levelText.text = playerSkillData.isNonUpgradeable ? string.Empty : $"Lv. {spellData.levelForDisplay.ToString()}";
        currenciesText.text = currencyStr;
        
        string bonusesStr = GetBonusesString(playerSkillData, spellData.currentLevel);
        bonusesStr = $"{bonusesStr}{GetAdditionalBonusesString(spellData, playerSkillData, spellData.currentLevel)}";
        bonusesText.text = bonusesStr;
        
        additionalText.text = GetAdditionalInfo(spellData);

        if (spellData is BrainwashData && UIManager.Instance.structureRoomInfoUI.isShowing && UIManager.Instance.structureRoomInfoUI.activeRoom is PrisonCell defilerRoom && defilerRoom.charactersInRoom.Count > 0) {
            Character targetCharacter = defilerRoom.charactersInRoom.First();
            if (targetCharacter != null) {
                fullDescription += $"\n<b>{targetCharacter.name} Brainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";    
            }
        }
        
        descriptionText.SetTextAndReplaceWithIcons(fullDescription);
    }
    private void UpdateData(string title, string description) {
        titleText.text = title;
        descriptionText.SetTextAndReplaceWithIcons(description);
        currenciesText.text = string.Empty;
        additionalText.text = string.Empty;
    }
    private void UpdateDataWithLevelUpDetails(SkillData spellData) {
        titleText.text = spellData.name;
        string fullDescription = spellData.description;
        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(spellData.type);
        
        levelText.text = playerSkillData.isNonUpgradeable ? string.Empty : 
            $"Lv. {spellData.levelForDisplay.ToString()} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText($"Lv. {(spellData.levelForDisplay + 1).ToString()}")}";
        
        string currencyStr = GetCurrencyLevelUpSummary(spellData, playerSkillData);
        currenciesText.text = currencyStr;
        
        string bonusesStr = GetBonusesLevelUpString(playerSkillData, spellData.currentLevel);
        bonusesStr = $"{bonusesStr}{GetAdditionalBonusesLevelUpString(spellData, playerSkillData, spellData.currentLevel)}";

        bonusesText.text = bonusesStr;
        // if (spellData.maxCharges > 0) {
        //     //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
        //     bonusesText.text = $"{bonusesText.text}{UtilityScripts.Utilities.ColorizeSpellTitle("Charges:")} {charges.ToString()}/{spellData.maxCharges.ToString()}";
        // }
        additionalText.text = GetAdditionalInfo(spellData);

        if (spellData is BrainwashData && UIManager.Instance.structureRoomInfoUI.isShowing && UIManager.Instance.structureRoomInfoUI.activeRoom is PrisonCell defilerRoom && defilerRoom.charactersInRoom.Count > 0) {
            Character targetCharacter = defilerRoom.charactersInRoom.First();
            if (targetCharacter != null) {
                fullDescription += $"\n<b>{targetCharacter.name} Brainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";    
            }
        }
        
        descriptionText.SetTextAndReplaceWithIcons(fullDescription);
    }

    private bool HasEnoughMana(SkillData spellData) {
        if (spellData.hasManaCost) {
            if (PlayerManager.Instance.player.mana >= spellData.manaCost) {
                return true;
            }
            return false;
        }
        //if skill has no mana cost then always has enough mana
        return true;
    }
    private bool HasEnoughCharges(SkillData spellData) {
        if (spellData.hasCharges) {
            if (spellData.charges > 0) {
                return true;
            }
            return false;
        }
        //if skill has no charges then always has enough charges
        return true;
    }
    private bool HasEnoughMana(int manaCost) {
        if (PlayerManager.Instance.player.mana >= manaCost) {
            return true;
        }
        return false;
    }
    private bool HasEnoughCharges(int charges) {
        if (charges > 0) {
            return true;
        }
        return false;
    }
    private string GetBonusesString(PlayerSkillData p_data, int p_level) {
        string bonuses = string.Empty;
        List<UPGRADE_BONUS> orderedBonuses = RuinarchListPool<UPGRADE_BONUS>.Claim();
        orderedBonuses.AddRange(p_data.skillUpgradeData.bonuses.OrderBy(u => u.GetUpgradeOrderInTooltip()));
        for (int i = 0; i < orderedBonuses.Count; i++) {
            UPGRADE_BONUS bonus = orderedBonuses[i];
            if (GetFormattedBonusString(bonus, p_data, p_level, out string formatted)) {
                bonuses = $"{bonuses}{formatted}\n";
                // if (!orderedBonuses.IsLastIndex(i)) {
                //     bonuses = $"{bonuses}\n";
                // }
            }
        }
        RuinarchListPool<UPGRADE_BONUS>.Release(orderedBonuses);
        return bonuses;
    }
    /// <summary>
    /// Get the formatted string for a bonus type.
    /// </summary>
    /// <param name="bonus">The bonus type.</param>
    /// <param name="p_data">The corresponding data for the current skill.</param>
    /// <param name="p_level">The level of the bonus to display.</param>
    /// <param name="formatted">the formatted string</param>
    /// <returns>True if bonus value is greater than 0, False if bonus value is 0 or bonus has no specified format.</returns>
    private bool GetFormattedBonusString(UPGRADE_BONUS bonus, PlayerSkillData p_data, int p_level, out string formatted) {
        formatted = UtilityScripts.Utilities.ColorizeSpellTitle($"{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(bonus.ToString())}: ");
        switch (bonus) {
            case UPGRADE_BONUS.Damage:
                formatted = $"{formatted}{p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_level).ToString()}";
                break;
            case UPGRADE_BONUS.Pierce:
                float value = p_data.skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
                formatted = $"{UtilityScripts.Utilities.ColorizeSpellTitle($"Piercing{UtilityScripts.Utilities.PiercingIcon()}:")} {value.ToString()}";
                if (value == 0f) {
                    return false; //skip 0 values;
                }
                break;
            case UPGRADE_BONUS.Duration:
                int durationInTicks = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_level);
                formatted = $"{formatted}{GameManager.ConvertTicksToWholeTime(durationInTicks)}";
                // formatted = $"{formatted}{GameManager.GetTimeAsWholeDuration(durationInTicks).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(durationInTicks)}";
                break;
            case UPGRADE_BONUS.Tile_Range:
                int tileRangeBonusPerLevel = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
                formatted = $"{formatted}{GetTileRangeDisplay(tileRangeBonusPerLevel)}";
                break;
            case UPGRADE_BONUS.Skill_Movement_Speed:
                formatted = $"{formatted}{p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level).ToString()}";
                break;
            case UPGRADE_BONUS.Cooldown:
                int cooldownInTicks = p_data.skillUpgradeData.GetCoolDownPerLevel(p_level);
                formatted = $"{formatted}{GameManager.ConvertTicksToWholeTime(cooldownInTicks)}";
                // formatted = $"{formatted}{GameManager.GetTimeAsWholeDuration(cooldownInTicks).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldownInTicks)}";
                break;
            default:
                formatted = string.Empty;
                return false;
        }
        return true;
    }
    private string GetAdditionalBonusesString(SkillData spellData, PlayerSkillData p_data, int p_level) {
        string additionalBonusesStr = string.Empty;
        if (spellData.maxCharges > 0) {
            //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
            additionalBonusesStr = $"{additionalBonusesStr}{UtilityScripts.Utilities.ColorizeSpellTitle("Max Charges:")} {spellData.maxCharges.ToString()}";
        }
        return additionalBonusesStr;
    }
    private string GetBonusDifferenceString(UPGRADE_BONUS bonus, PlayerSkillData p_data, int p_level, int p_nextLevel) {
        string differenceStr = string.Empty;
        int intCurrentValue;
        float fltCurrentValue;
        int intNextValue;
        float fltNextValue;
        int intDifference;
        float fltDifference;
        switch (bonus) {
            case UPGRADE_BONUS.Damage:
                intCurrentValue = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_level);
                intNextValue = p_data.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_nextLevel);
                intDifference = intNextValue - intCurrentValue;
                if (intDifference > 0) {
                    differenceStr = $"{intNextValue.ToString()}";    
                }
                break;
            case UPGRADE_BONUS.Pierce:
                fltCurrentValue = p_data.skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
                fltNextValue = p_data.skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_nextLevel);
                fltDifference = fltNextValue - fltCurrentValue;
                if (fltDifference > 0) {
                    differenceStr = $"{fltNextValue.ToString()}";    
                }    
                
                break;
            case UPGRADE_BONUS.Duration:
                intCurrentValue = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_level);
                intNextValue = p_data.skillUpgradeData.GetDurationBonusPerLevel(p_nextLevel);
                if (intCurrentValue != intNextValue) {
                    differenceStr = $"{GameManager.ConvertTicksToWholeTime(intNextValue)}";
                    // differenceStr = $" ({GameManager.GetTimeAsWholeDuration(intNextValue).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(intNextValue)})";
                }
                break;
            case UPGRADE_BONUS.Tile_Range:
                intCurrentValue = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
                intNextValue = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_nextLevel);
                intDifference = intNextValue - intCurrentValue;
                if (intDifference > 0) {
                    differenceStr = $"{GetTileRangeDisplay(intNextValue)}";
                }
                break;
            case UPGRADE_BONUS.Skill_Movement_Speed:
                intCurrentValue = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level);
                intNextValue = p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_nextLevel);
                intDifference = intNextValue - intCurrentValue;
                if (intDifference > 0) {
                    differenceStr = $"{intNextValue.ToString()}";
                }
                break;
            case UPGRADE_BONUS.Cooldown:
                intCurrentValue = p_data.skillUpgradeData.GetCoolDownPerLevel(p_level);
                intNextValue = p_data.skillUpgradeData.GetCoolDownPerLevel(p_nextLevel);
                if (intCurrentValue != intNextValue) {
                    differenceStr = $"{GameManager.ConvertTicksToWholeTime(intNextValue)}";
                    // differenceStr = $" ({GameManager.GetTimeAsWholeDuration(intNextValue).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(intNextValue)})";
                }
                break;
            default:
                differenceStr = string.Empty;
                break;
        }
        return differenceStr;
    }
    private string GetBonusesLevelUpString(PlayerSkillData p_data, int p_level) {
        string bonuses = string.Empty;
        List<UPGRADE_BONUS> orderedBonuses = RuinarchListPool<UPGRADE_BONUS>.Claim();
        orderedBonuses.AddRange(p_data.skillUpgradeData.bonuses.OrderBy(u => u.GetUpgradeOrderInTooltip()));
        for (int i = 0; i < orderedBonuses.Count; i++) {
            UPGRADE_BONUS bonus = orderedBonuses[i];
            GetFormattedBonusString(bonus, p_data, p_level, out string formatted);
            if (!string.IsNullOrEmpty(formatted)) {
                string differenceString = GetBonusDifferenceString(bonus, p_data, p_level, p_level + 1);
                if (!string.IsNullOrEmpty(differenceString)) {
                    formatted = $"{formatted} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText($"{differenceString}")}";
                }
                bonuses = $"{bonuses}{formatted}\n";
                // if (!orderedBonuses.IsLastIndex(i)) {
                //     bonuses = $"{bonuses}\n";
                // }
            }
        }
        RuinarchListPool<UPGRADE_BONUS>.Release(orderedBonuses);
        return bonuses;
    }
    private string GetAdditionalBonusesLevelUpString(SkillData spellData, PlayerSkillData p_data, int p_level) {
        int nextLevel = p_level + 1;
        string additionalBonusesStr = string.Empty;
        if (spellData.maxCharges > 0) {
            //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
            additionalBonusesStr = $"{additionalBonusesStr}{UtilityScripts.Utilities.ColorizeSpellTitle("Max Charges:")} {spellData.maxCharges.ToString()}";
            
            int currentMaxCharges = spellData.maxCharges;
            int nextMaxCharges = p_data.skillUpgradeData.GetChargesBaseOnLevel(nextLevel);
            int difference = nextMaxCharges - currentMaxCharges;
            if (difference > 0) {
                additionalBonusesStr = $"{additionalBonusesStr} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText($"{nextMaxCharges.ToString()}")}";
            }
        }
        return additionalBonusesStr;
    }
    private string GetAdditionalInfo(SkillData spellData) {
        string additionalText = string.Empty;
        if (spellData is PlayerAction) {
            object activeObj = UIManager.Instance.GetCurrentlySelectedObject() ?? PlayerManager.Instance.player.currentlySelectedPlayerActionTarget;
            if (activeObj != null) {
                if (activeObj is Character activeCharacter) {
                    if (spellData.CanPerformAbilityTowards(activeCharacter) == false) {
                        if (spellData is PlayerAction playerAction && !playerAction.canBeCastOnBlessed && activeCharacter.traitContainer.IsBlessed()) {
                            additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Blessed Villagers are protected from your powers.")}\n";
                        }
                        string wholeReason = spellData
                            .GetReasonsWhyCannotPerformAbilityTowards(activeCharacter);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                } else if (activeObj is TileObject activeTileObject) {
                    if (activeTileObject is AnkhOfAnubis ankh && ankh.isActivated && spellData.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT) {
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Activated Ankh can no longer be seized.")}\n";
                    }
                } else if (activeObj is BaseSettlement activeSettlement) {
                    if (spellData.CanPerformAbilityTowards(activeSettlement) == false) {
                        string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeSettlement);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                } else if (activeObj is LocationStructure activeStructure) {
                    if (spellData.CanPerformAbilityTowards(activeStructure) == false) {
                        string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeStructure);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                }
            }
        }
        if(HasEnoughMana(spellData) == false) {
            additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough mana.")}\n";
        }
        if(HasEnoughCharges(spellData) == false) {
            if (spellData.hasCooldown) {
                additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Recharging.")}\n";
            } else {
                additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough charges.")}\n";
            }
        }
        
        return additionalText;
    }
    private string GetCurrencySummary(int manaCost, int charges, int maxCharges, int cooldown) {
        string currencies = string.Empty;
        if (charges > 0) {
            currencies = $"{currencies}{charges.ToString()}/{maxCharges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}    ";
        }
        if (manaCost > 0) {
            currencies = $"{currencies}{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";
        }
        // if (maxCharges > 0) {
        //     currencies = $"{currencies}{charges.ToString()}/{maxCharges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        // }
        // if (cooldown > 0) {
        //     currencies = $"{currencies}{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        // }
        return currencies;
    }
    private string GetCurrencyLevelUpSummary(SkillData skillData, PlayerSkillData playerSkillData) {
        string currencies = string.Empty;
        if (skillData.hasCharges) {
            currencies = $"{currencies}{skillData.charges.ToString()}/{skillData.maxCharges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}    ";
        }
        
        int currentManaCost = skillData.manaCost;
        int nextLevelManaCost = playerSkillData.GetManaCostBaseOnLevel(skillData.currentLevel + 1);
        if (currentManaCost != nextLevelManaCost) {
            currencies = $"{currencies}{currentManaCost.ToString()} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText(nextLevelManaCost.ToString())} {UtilityScripts.Utilities.ManaIcon()}";
        }
        return currencies;
    }
    private string GetTileRangeDisplay(int radius) {
        int dimension = (radius * 2) + 1;
        return $"{dimension.ToString()}x{dimension.ToString()}";
    }
}
