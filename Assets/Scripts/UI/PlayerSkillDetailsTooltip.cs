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

    public void ShowPlayerSkillDetails(SkillData spellData, UIHoverPosition position = null, bool p_dontShowAdditionalText = false) {
        UpdateData(spellData, p_dontShowAdditionalText);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillDetails(PlayerSkillData skillData, int level = 0, UIHoverPosition position = null) {
        UpdateData(skillData, level);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillDetails(string title, string description, UIHoverPosition position = null) {
        UpdateData(title, description);
        UpdatePosition(position);
    }
    public void ShowPlayerSkillWithLevelUpDetails(SkillData spellData, UIHoverPosition position = null, bool p_isChaoticEnergyEnough = true) {
        UpdateDataWithLevelUpDetails(spellData, p_isChaoticEnergyEnough);
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
    private void UpdateData(PlayerSkillData p_playerSkillData, int level) {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(p_playerSkillData.skill);
        titleText.SetText(skillData.name);
        descriptionText.SetTextAndReplaceWithIcons(skillData.description);
        int charges = skillData.maxCharges;// SpellUtilities.GetModifiedSpellCost(, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeCostsModification());
        int manaCost = skillData.manaCost;// SpellUtilities.GetModifiedSpellCost(, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
        int cooldown = skillData.cooldown;// SpellUtilities.GetModifiedSpellCost(, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification());
        //int threat = SpellUtilities.GetModifiedSpellCost(skillData.threat, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetThreatModification());

        //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
        string currencyStr = GetCurrencySummary(manaCost, charges, charges, cooldown, skillData.bonusCharges, skillData.isInUse); 
        
        levelText.text = $"Lv. {(level + 1).ToString()}";
        currenciesText.text = currencyStr;
        additionalText.text = string.Empty;
        bonusesText.text = GetBonusesString(p_playerSkillData, level);
        if (charges > 0) {
            //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
            bonusesText.text = $"{bonusesText.text}{UtilityScripts.Utilities.ColorizeSpellTitle("Charges:")} {charges.ToString()}/{charges.ToString()}";
        }
        if (p_playerSkillData.resistanceType != RESISTANCE.None) {
            ELEMENTAL_TYPE elementalType = p_playerSkillData.resistanceType.GetElement();
            bonusesText.text = $"{bonusesText.text}{UtilityScripts.Utilities.ColorizeSpellTitle("Element:")}{UtilityScripts.Utilities.GetRichTextIconForElement(elementalType)}";
        }
        if (p_playerSkillData.isAffliction) {
            bonusesText.text = $"{bonusesText.text}\n{GetAfflictionBonusesString(p_playerSkillData, 1)}";
        } else {
            bonusesText.text = $"{bonusesText.text}\n{GetPlayerActionSkillBonusesString(p_playerSkillData, 1)}";
        }
        
    }
    private void UpdateData(SkillData skillData, bool p_dontShowAdditionalText = false) {
        // UnityEngine.Debug.LogError(skillData.name + " -- " + skillData.manaCost);
        titleText.text = skillData.name;
        string fullDescription = skillData.description;
        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(skillData.type);
        
        string currencyStr = GetCurrencySummary(manaCost, charges, skillData.maxCharges, cooldown, skillData.bonusCharges, skillData.isInUse);
        levelText.text = playerSkillData.isNonUpgradeable ? string.Empty : $"Lv. {skillData.levelForDisplay.ToString()}";
        currenciesText.text = currencyStr;
        
        string bonusesStr = GetBonusesString(playerSkillData, skillData.currentLevel);
        bonusesStr = $"{bonusesStr}{GetAdditionalBonusesString(skillData, playerSkillData, skillData.currentLevel)}";
        bonusesText.text = bonusesStr;
        if (playerSkillData.isAffliction) {
            bonusesText.text = $"{bonusesText.text}\n{GetAfflictionBonusesString(playerSkillData, skillData.currentLevel)}";
        } else {
            bonusesText.text = $"{bonusesText.text}\n{GetPlayerActionSkillBonusesString(playerSkillData, skillData.currentLevel)}";
        }
        

        if (p_dontShowAdditionalText) {
            additionalText.text = string.Empty;
        } else {
            additionalText.text = GetAdditionalInfo(skillData);
        }
        
        if (skillData is BrainwashData) {
            Character targetCharacter = null;
            if (UIManager.Instance.structureRoomInfoUI.isShowing && UIManager.Instance.structureRoomInfoUI.activeRoom is PrisonCell prisonCell && prisonCell.HasCharacterInRoom()) {
                targetCharacter = prisonCell.GetFirstAliveCharacterInRoom();
            } else if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is Character character) {
                targetCharacter = character;
            } else if (PlayerManager.Instance.player.currentlySelectedPlayerActionTarget is PrisonCell room && room.HasCharacterInRoom()) {
                targetCharacter = room.GetFirstAliveCharacterInRoom();    
            } else if (UIManager.Instance.structureInfoUI.isShowing && UIManager.Instance.structureInfoUI.activeStructure is TortureChambers tortureChambers &&
                       tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell cell && cell.HasCharacterInRoom()) {
                targetCharacter = cell.GetFirstAliveCharacterInRoom();    
            }
            if (targetCharacter != null) {
                fullDescription = $"{fullDescription}\n<b>{targetCharacter.name} Brainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";    
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
    private void UpdateDataWithLevelUpDetails(SkillData spellData, bool p_isChaoticEnergyEnough = true) {
        titleText.text = spellData.name;
        string fullDescription = spellData.description;
        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
        
        levelText.text = playerSkillData.isNonUpgradeable ? string.Empty : 
            $"Lv. {spellData.levelForDisplay.ToString()} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText($"Lv. {(spellData.levelForDisplay + 1).ToString()}")}";
        
        string currencyStr = GetCurrencyLevelUpSummary(spellData, playerSkillData);
        currenciesText.text = currencyStr;
        
        string bonusesStr = GetBonusesLevelUpString(playerSkillData, spellData.currentLevel);
        string additionalBonusesLevelUpString = GetAdditionalBonusesLevelUpString(spellData, playerSkillData, spellData.currentLevel);
        if (!string.IsNullOrEmpty(additionalBonusesLevelUpString)) {
            bonusesStr = $"{bonusesStr}\n{additionalBonusesLevelUpString}";    
        }
        string afflictionBonusesString = string.Empty;
        if (playerSkillData.isAffliction) {
            afflictionBonusesString = GetAfflictionBonusesWithLevelUpDetailsString(playerSkillData, spellData.currentLevel);
        } else {
            afflictionBonusesString = GetPlayerActionSkillBonusesWithLevelUpDetailsString(playerSkillData, spellData.currentLevel);
        }
        
        if (!string.IsNullOrEmpty(afflictionBonusesString)) {
            bonusesStr = $"{bonusesStr}\n{afflictionBonusesString}";    
        }
        bonusesText.text = bonusesStr;
        // if (spellData.maxCharges > 0) {
        //     //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
        //     bonusesText.text = $"{bonusesText.text}{UtilityScripts.Utilities.ColorizeSpellTitle("Charges:")} {charges.ToString()}/{spellData.maxCharges.ToString()}";
        // }
        additionalText.text = GetAdditionalInfoForSpire(p_isChaoticEnergyEnough);

        // if (spellData is BrainwashData && UIManager.Instance.structureRoomInfoUI.isShowing && UIManager.Instance.structureRoomInfoUI.activeRoom is PrisonCell defilerRoom && defilerRoom.charactersInRoom.Count > 0) {
        //     Character targetCharacter = defilerRoom.charactersInRoom.First();
        //     if (targetCharacter != null) {
        //         fullDescription += $"\n<b>{targetCharacter.name} Brainwash Success Rate: {PrisonCell.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";    
        //     }
        // }

        descriptionText.SetTextAndReplaceWithIcons(fullDescription);
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

    #region Bonuses
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
                float value = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
                formatted = $"{UtilityScripts.Utilities.ColorizeSpellTitle($"Piercing{UtilityScripts.Utilities.PiercingIcon()}:")} {value.ToString()}";
                if (value == 0f) {
                    return false; //skip 0 values;
                }
                break;
            case UPGRADE_BONUS.Duration:
                int durationInTicks = p_data.GetDurationBonusPerLevel(p_level);
                formatted = $"{formatted}{GameManager.ConvertTicksToWholeTime(durationInTicks)}";
                break;
            case UPGRADE_BONUS.Tile_Range:
                int tileRangeBonusPerLevel = p_data.skillUpgradeData.GetTileRangeBonusPerLevel(p_level);
                formatted = $"{formatted}{GetTileRangeDisplay(tileRangeBonusPerLevel)}";
                break;
            case UPGRADE_BONUS.Skill_Movement_Speed:
                formatted = $"{formatted}{p_data.skillUpgradeData.GetSkillMovementSpeedPerLevel(p_level).ToString()}";
                break;
            case UPGRADE_BONUS.Cooldown:
                int cooldownInTicks = p_data.GetCoolDownBaseOnLevel(p_level);
                if (cooldownInTicks == 0) {
                    formatted = $"{formatted}Instant";
                } else if (cooldownInTicks == -1) {
                    formatted = $"{formatted}No Cooldown";
                } else {
                    formatted = $"{formatted}{GameManager.ConvertTicksToWholeTime(cooldownInTicks)}";    
                }
                break;
            case UPGRADE_BONUS.Atk_Percentage:
                formatted = UtilityScripts.Utilities.ColorizeSpellTitle("Strength Increase: ");
                formatted = $"{formatted}{p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_level).ToString(CultureInfo.InvariantCulture)}%";
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
            additionalBonusesStr = $"{additionalBonusesStr}{UtilityScripts.Utilities.ColorizeSpellTitle("Max Charges:")} {spellData.maxCharges.ToString()}\n";
        }
        if (p_data.resistanceType != RESISTANCE.None) {
            ELEMENTAL_TYPE elementalType = p_data.resistanceType.GetElement();
            additionalBonusesStr = $"{additionalBonusesStr}{UtilityScripts.Utilities.ColorizeSpellTitle("Element:")} {UtilityScripts.Utilities.GetRichTextIconForElement(elementalType)}";
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
                fltCurrentValue = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_level);
                fltNextValue = p_data.GetAdditionalPiercePerLevelBaseOnLevel(p_nextLevel);
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
                intCurrentValue = p_data.GetCoolDownBaseOnLevel(p_level);
                intNextValue = p_data.GetCoolDownBaseOnLevel(p_nextLevel);
                if (intCurrentValue != intNextValue) {
                    if (intNextValue == 0) {
                        differenceStr = "Instant";
                    } else if (intNextValue == -1) {
                        differenceStr = "No Cooldown";
                    } else {
                        differenceStr = $"{GameManager.ConvertTicksToWholeTime(intNextValue)}";    
                    }
                    // differenceStr = $" ({GameManager.GetTimeAsWholeDuration(intNextValue).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(intNextValue)})";
                }
                break;
            case UPGRADE_BONUS.Atk_Percentage:
                fltCurrentValue = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_level);
                fltNextValue = p_data.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(p_nextLevel);
                fltDifference = fltNextValue - fltCurrentValue;
                if (fltDifference > 0) {
                    differenceStr = $"{fltNextValue.ToString()}%";    
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
                bonuses = $"{bonuses}{formatted}";
                // if (!orderedBonuses.IsLastIndex(i)) {
                    bonuses = $"{bonuses}\n";
                // }
            } else {
                bonuses = bonuses.TrimEnd();
            }
        }
        RuinarchListPool<UPGRADE_BONUS>.Release(orderedBonuses);
        return bonuses.TrimEnd();
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
        
        if (p_data.resistanceType != RESISTANCE.None) {
            ELEMENTAL_TYPE elementalType = p_data.resistanceType.GetElement();
            additionalBonusesStr = $"{additionalBonusesStr}\n{UtilityScripts.Utilities.ColorizeSpellTitle("Element:")}{UtilityScripts.Utilities.GetRichTextIconForElement(elementalType)}";
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
                        if (spellData is PlayerAction playerAction && !playerAction.GetCanBeCastOnBlessed() && activeCharacter.traitContainer.IsBlessed()) {
                            additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Blessed Villagers are protected from your powers.")}\n";
                        }
                        string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeCharacter);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                } else if (activeObj is TileObject activeTileObject) {
                    if (activeTileObject is AnkhOfAnubis ankh && ankh.isActivated && spellData.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT) {
                        additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Activated Ankh can no longer be seized.")}\n";
                    } else if (activeTileObject is Tombstone tombstone && tombstone.character != null) {
                        if (!spellData.CanPerformAbilityTowards(tombstone.character)) {
                            string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(tombstone.character);
                            wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                            additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                        }
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

    private string GetAdditionalInfoForSpire(bool p_isChaoticEnergyEnough = true) {
        string additionalText = string.Empty;
        if (!p_isChaoticEnergyEnough) {
            additionalText = $"{additionalText}{UtilityScripts.Utilities.ColorizeInvalidText("Not enough Chaotic Energy.")}\n";
        }
        return additionalText;
    }

    private string GetTileRangeDisplay(int radius) {
        int dimension = (radius * 2) + 1;
        return $"{dimension.ToString()}x{dimension.ToString()}";
    }
    #endregion

    #region Currency
    private string GetCurrencySummary(int manaCost, int charges, int maxCharges, int cooldown, int bonusCharges, bool isInUse) {
        string currencies = string.Empty;
        string notCombinedChargesText = SpellUtilities.GetDisplayOfCurrentChargesWithBonusChargesNotCombined(charges, maxCharges, bonusCharges, maxCharges > 0 && isInUse);
        if (!string.IsNullOrEmpty(notCombinedChargesText)) {
            currencies = $"{currencies}{notCombinedChargesText}    ";
        }
        if (manaCost > 0) {
            currencies = $"{currencies}{manaCost} {UtilityScripts.Utilities.ManaIcon()}";
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
        string notCombinedChargesText = skillData.displayOfCurrentChargesWithBonusChargesNotCombined;
        if (!string.IsNullOrEmpty(notCombinedChargesText)) {
            currencies = $"{currencies}{notCombinedChargesText}    ";
        }

        int currentManaCost = skillData.manaCost;
        int nextLevelManaCost = WorldSettings.Instance.worldSettingsData.IsScenarioMap()
            ? playerSkillData.GetManaCostForScenarios()
            : playerSkillData.GetManaCostBaseOnLevel(skillData.currentLevel + 1);//playerSkillData.GetManaCostBaseOnLevel(skillData.currentLevel + 1);
        if (currentManaCost != nextLevelManaCost) {
            currencies = $"{currencies}{currentManaCost} {UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText(nextLevelManaCost.ToString())} {UtilityScripts.Utilities.ManaIcon()}";
        } else {
            currencies = $"{currencies}{currentManaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}";
        }
        return currencies;
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
    #endregion

    #region Affliction
    private string GetAfflictionBonusesString(PlayerSkillData p_data, int p_level) {
        string bonuses = string.Empty;
        for (int i = 0; i < p_data.afflictionUpgradeData.bonuses.Count; i++) {
            AFFLICTION_UPGRADE_BONUS afflictionUpgradeBonus = p_data.afflictionUpgradeData.bonuses[i];
            string formattedString = GetFormattedAfflictionBonusString(afflictionUpgradeBonus, p_data, p_level);
            if (!string.IsNullOrEmpty(formattedString)) {
                bonuses = $"{bonuses}{formattedString}\n";
            }
        }
        return bonuses.TrimEnd();
    }

    private string GetPlayerActionSkillBonusesString(PlayerSkillData p_data, int p_level) {
        string bonuses = string.Empty;
        SkillData skilldata = PlayerSkillManager.Instance.GetSkillData(p_data.skill);
        if (skilldata is RemoveBuffData && p_level >= 3) {
            bonuses = $"{UtilityScripts.Utilities.ColorizeSpellTitle("Can remove Blessed Trait.")}\n";
        }
        return bonuses.TrimEnd();
    }

    private string GetPlayerActionSkillBonusesWithLevelUpDetailsString(PlayerSkillData p_data, int p_level) {
        SkillData skilldata = PlayerSkillManager.Instance.GetSkillData(p_data.skill);
        string str = string.Empty;
        if (skilldata is RemoveBuffData && p_level >= 2) {
            str = $"{UtilityScripts.Utilities.ColorizeUpgradeText("Can remove Blessed Trait.")}\n"; ;
        }
        return str;
    }
    private string GetAfflictionBonusesWithLevelUpDetailsString(PlayerSkillData p_data, int p_level) {
        string bonuses = string.Empty;
        for (int i = 0; i < p_data.afflictionUpgradeData.bonuses.Count; i++) {
            AFFLICTION_UPGRADE_BONUS afflictionUpgradeBonus = p_data.afflictionUpgradeData.bonuses[i];
            string formattedString = GetFormattedAfflictionBonusString(afflictionUpgradeBonus, p_data, p_level);
            int nextLevel = p_level + 1;
            if (afflictionUpgradeBonus == AFFLICTION_UPGRADE_BONUS.Added_Behaviour) {
                List<AFFLICTION_SPECIFIC_BEHAVIOUR> currentBehaviours = RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Claim();
                p_data.afflictionUpgradeData.PopulateAddedBehavioursForLevelNoDuplicates(currentBehaviours, p_level);
                AFFLICTION_SPECIFIC_BEHAVIOUR newBehaviour = p_data.afflictionUpgradeData.addedBehaviour[nextLevel];
                if (currentBehaviours.Contains(AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players)&& newBehaviour == AFFLICTION_SPECIFIC_BEHAVIOUR.Murder_Singers_Guitar_Players) {
                    formattedString = formattedString.Replace("knock out", UtilityScripts.Utilities.ColorizeUpgradeText("murder"));
                    bonuses = $"{bonuses}{formattedString}\n";
                } else {
                    bonuses = $"{bonuses}{formattedString}";
                    if (!string.IsNullOrEmpty(formattedString)) { bonuses = $"{bonuses}\n"; }
                    string differenceStr = GetAfflictionBonusDifferenceString(afflictionUpgradeBonus, p_data, p_level, nextLevel);
                    bonuses = $"{bonuses}{UtilityScripts.Utilities.ColorizeUpgradeText(differenceStr)}\n";    
                }
                RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Release(currentBehaviours);
            } else {
                if (!string.IsNullOrEmpty(formattedString)) {
                    bonuses = $"{bonuses}{formattedString}";
                    string differenceStr = GetAfflictionBonusDifferenceString(afflictionUpgradeBonus, p_data, p_level, nextLevel);
                    bonuses = $"{bonuses}{UtilityScripts.Utilities.UpgradeArrowIcon()} {UtilityScripts.Utilities.ColorizeUpgradeText(differenceStr)}\n";    
                }
            }
        }
        return bonuses.TrimEnd();
    }
    private string GetAfflictionBonusDifferenceString(AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus, PlayerSkillData p_data, int p_level, int p_nextLevel) {
        switch (p_afflictionUpgradeBonus) {
            case AFFLICTION_UPGRADE_BONUS.Crowd_Number:
                return $"{p_data.afflictionUpgradeData.crowdNumber[p_nextLevel].ToString()}";
            case AFFLICTION_UPGRADE_BONUS.Hunger_Rate:
                return $"+{p_data.afflictionUpgradeData.hungerRate[p_nextLevel].ToString()}%";
            case AFFLICTION_UPGRADE_BONUS.Trigger_Rate:
                return $"{p_data.afflictionUpgradeData.rateChance[p_nextLevel].ToString()}%";
            case AFFLICTION_UPGRADE_BONUS.Naps_Duration:
                int napTicks = p_data.afflictionUpgradeData.napsDuration[p_nextLevel];
                return $"{GameManager.ConvertTicksToWholeTime(napTicks)}";
            case AFFLICTION_UPGRADE_BONUS.Duration:
                int afflictionDuration = (int)p_data.afflictionUpgradeData.duration[p_nextLevel];
                string timeStr = afflictionDuration == 0 ? "Permanent" : GameManager.ConvertTicksToWholeTime(afflictionDuration);
                return $"{timeStr}";
            case AFFLICTION_UPGRADE_BONUS.Number_Criteria:
                return $"{p_data.afflictionUpgradeData.numberOfCriteria[p_nextLevel].ToString()}";
            case AFFLICTION_UPGRADE_BONUS.Criteria:
                List<LIST_OF_CRITERIA> criteriaList = RuinarchListPool<LIST_OF_CRITERIA>.Claim();
                p_data.afflictionUpgradeData.PopulateAvailableCriteriaForLevel(criteriaList, p_nextLevel);
                string criteriaSummary = $"{criteriaList.ComafyList()}";
                RuinarchListPool<LIST_OF_CRITERIA>.Release(criteriaList);
                return criteriaSummary;
            case AFFLICTION_UPGRADE_BONUS.Trigger_Opinion:
                string allAddedOpinionTriggers = string.Empty;
                List<OPINIONS> triggers = RuinarchListPool<OPINIONS>.Claim();
                p_data.afflictionUpgradeData.PopulateOpinionTriggerForLevel(triggers, p_nextLevel);
                if (triggers.Contains(OPINIONS.Everyone)) {
                    allAddedOpinionTriggers = $"{allAddedOpinionTriggers}Everyone";
                } else {
                    for (int i = 0; i < triggers.Count; i++) {
                        OPINIONS opinions = triggers[i];
                        if (opinions == OPINIONS.NoOne) { continue;; }
                        allAddedOpinionTriggers = $"{allAddedOpinionTriggers}{opinions.ToString()}";
                        if (!triggers.IsLastIndex(i)) {
                            allAddedOpinionTriggers = $"{allAddedOpinionTriggers}, ";
                        }
                    }    
                }
                RuinarchListPool<OPINIONS>.Release(triggers);
                return allAddedOpinionTriggers;
            case AFFLICTION_UPGRADE_BONUS.Added_Behaviour:
                string allAddedBehaviours = string.Empty;
                List<AFFLICTION_SPECIFIC_BEHAVIOUR> currentBehaviours = RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Claim();
                p_data.afflictionUpgradeData.PopulateAddedBehavioursForLevelNoDuplicates(currentBehaviours, p_level);
                AFFLICTION_SPECIFIC_BEHAVIOUR newBehaviour = p_data.afflictionUpgradeData.addedBehaviour[p_nextLevel];
                if (currentBehaviours.Contains(newBehaviour) || newBehaviour == AFFLICTION_SPECIFIC_BEHAVIOUR.None) {
                    return string.Empty;
                } else {
                    string addedBehaviour = GetFormattedAfflictionSpecificBehaviour(newBehaviour);
                    if (!string.IsNullOrEmpty(addedBehaviour)) {
                        allAddedBehaviours = $"{allAddedBehaviours}{addedBehaviour}";
                    }
                }
                RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Release(currentBehaviours);
                if (!string.IsNullOrEmpty(allAddedBehaviours)) {
                    // return UtilityScripts.Utilities.ColorizeSpellTitle(allAddedBehaviours);
                    return allAddedBehaviours;
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }
    private string GetFormattedAfflictionBonusString(AFFLICTION_UPGRADE_BONUS p_afflictionUpgradeBonus, PlayerSkillData p_data, int p_level) {
        switch (p_afflictionUpgradeBonus) {
            case AFFLICTION_UPGRADE_BONUS.Crowd_Number:
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Crowd:")} {p_data.afflictionUpgradeData.crowdNumber[p_level].ToString()}";
            case AFFLICTION_UPGRADE_BONUS.Hunger_Rate:
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Hunger Rate:")} +{p_data.afflictionUpgradeData.hungerRate[p_level].ToString()}%";
            case AFFLICTION_UPGRADE_BONUS.Trigger_Rate:
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Trigger Rate:")} {p_data.afflictionUpgradeData.rateChance[p_level].ToString()}%";
            case AFFLICTION_UPGRADE_BONUS.Naps_Duration:
                int napTicks = p_data.afflictionUpgradeData.napsDuration[p_level];
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Nap Duration:")} {GameManager.ConvertTicksToWholeTime(napTicks)}";
            case AFFLICTION_UPGRADE_BONUS.Duration:
                int afflictionDuration = (int)p_data.afflictionUpgradeData.duration[p_level];
                string timeStr = afflictionDuration == 0 ? "Permanent" : GameManager.ConvertTicksToWholeTime(afflictionDuration);
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Status Duration:")} {timeStr}";
            case AFFLICTION_UPGRADE_BONUS.Number_Criteria:
                return $"{UtilityScripts.Utilities.ColorizeSpellTitle("Criteria:")} {p_data.afflictionUpgradeData.numberOfCriteria[p_level].ToString()}";
            case AFFLICTION_UPGRADE_BONUS.Criteria:
                string criteriaSummary = UtilityScripts.Utilities.ColorizeSpellTitle("Available Criteria: ");
                List<LIST_OF_CRITERIA> criteriaList = RuinarchListPool<LIST_OF_CRITERIA>.Claim();
                p_data.afflictionUpgradeData.PopulateAvailableCriteriaForLevel(criteriaList, p_level);
                criteriaSummary = $"{criteriaSummary}{criteriaList.ComafyList()}";
                RuinarchListPool<LIST_OF_CRITERIA>.Release(criteriaList);
                return criteriaSummary;
            case AFFLICTION_UPGRADE_BONUS.Trigger_Opinion:
                string allAddedOpinionTriggers = UtilityScripts.Utilities.ColorizeSpellTitle("Triggers: ");
                List<OPINIONS> triggers = RuinarchListPool<OPINIONS>.Claim();
                p_data.afflictionUpgradeData.PopulateOpinionTriggerForLevel(triggers, p_level);
                if (triggers.Contains(OPINIONS.Everyone)) {
                    allAddedOpinionTriggers = $"{allAddedOpinionTriggers}Everyone";
                } else {
                    for (int i = 0; i < triggers.Count; i++) {
                        OPINIONS opinions = triggers[i];
                        if (opinions == OPINIONS.NoOne) { continue;; }
                        allAddedOpinionTriggers = $"{allAddedOpinionTriggers}{opinions.ToString()}";
                        if (!triggers.IsLastIndex(i)) {
                            allAddedOpinionTriggers = $"{allAddedOpinionTriggers}, ";
                        }
                    }    
                }
                RuinarchListPool<OPINIONS>.Release(triggers);
                return allAddedOpinionTriggers;
            case AFFLICTION_UPGRADE_BONUS.Added_Behaviour:
                string allAddedBehaviours = string.Empty;
                List<AFFLICTION_SPECIFIC_BEHAVIOUR> behaviours = RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Claim();
                p_data.afflictionUpgradeData.PopulateAddedBehavioursForLevelNoDuplicates(behaviours, p_level);
                for (int i = 0; i < behaviours.Count; i++) {
                    AFFLICTION_SPECIFIC_BEHAVIOUR behaviour = behaviours[i];
                    if (behaviour == AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players && behaviours.Contains(AFFLICTION_SPECIFIC_BEHAVIOUR.Murder_Singers_Guitar_Players)) {
                        //skip knock out singers if murder singers is already part of the added behaviours list
                        continue;
                    }
                    string addedBehaviour = GetFormattedAfflictionSpecificBehaviour(behaviour);
                    if (!string.IsNullOrEmpty(addedBehaviour)) {
                        allAddedBehaviours = $"{allAddedBehaviours}{addedBehaviour}\n";
                    }
                }
                allAddedBehaviours = allAddedBehaviours.TrimEnd();
                RuinarchListPool<AFFLICTION_SPECIFIC_BEHAVIOUR>.Release(behaviours);
                if (!string.IsNullOrEmpty(allAddedBehaviours)) {
                    return UtilityScripts.Utilities.ColorizeSpellTitle(allAddedBehaviours);    
                }
                return string.Empty;
            default:
                return string.Empty;
        }
    }
    private string GetFormattedAfflictionSpecificBehaviour(AFFLICTION_SPECIFIC_BEHAVIOUR p_behaviour) {
        switch (p_behaviour) {
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Make_Anxious:
                return "Makes character Anxious";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.No_Longer_Join_Parties:
                return "Will no longer joins Parties";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Ignore_Urgent_Tasks:
                return "May ignore urgent tasks";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Angry_Upon_Hear_Music:
                return "Always gets Angry when hearing music";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Knockout_Singers_Guitar_Players:
                return "Will knock out singers and guitar players";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Murder_Singers_Guitar_Players:
                return "Will murder singers and guitar players";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Active_Search_Affair:
                return "Actively searches for an Affair";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Multiple_Affair:
                return "Actively searches for multiple Affairs";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair:
                return "Actively searches for multiple Affairs. Anyone will do";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Pass_Out_From_Fright:
                return "May pass out from fright";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Flees_From_Anyone:
                return "Flees from anyone they don't know";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Do_Pick_Pocket:
                return "May pickpocket other Villagers";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Rob_From_House:
                return "May rob from other houses";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Rob_Any_Place:
                return "May rob any place";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.May_Suffer_heart_Attack:
                return "May suffer heart attack from fright";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Becomes_Stronger_Dire_wolf:
                return "Becomes a stronger Dire Wolf while in Wolf form.";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Transform_Master_Werewolf:
                return "Transforms into a Master Werewolf";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Form_Lycan_Clan_faction:
                return "Werewolves may now form a Lycan Clan faction";
            // case AFFLICTION_SPECIFIC_BEHAVIOUR.Add_And_Selection:
            //     return "Includes AND setting only";
            // case AFFLICTION_SPECIFIC_BEHAVIOUR.Add_Or_Selection:
            //     return "Includes AND or OR dropdown";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Transform_into_bats:
                return "Vampires may transform into bats";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Can_Become_Vampire_Lords:
                return "Vampires may eventually become Vampire Lords";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Create_Vampire_Clan_Faction:
                return "Vampires may now form a Vampire Clan faction";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Likes_To_Sleep:
                return "Likes to take naps";
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Loves_To_Sleep:
                return "Loves to take naps";
            default:
                return string.Empty;
        }
    }
    #endregion
}
