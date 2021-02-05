﻿using System.Collections;
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
    public RuinarchText descriptionText;
    public TextMeshProUGUI currenciesText;
    public TextMeshProUGUI additionalText;
    public UIHoverPosition defaultPosition;
    public RawImage tooltipImage;
    public VideoPlayer tooltipVideoPlayer;
    public RenderTexture tooltipVideoRenderTexture;

    public void ShowPlayerSkillDetails(SkillData spellData, UIHoverPosition position = null) {
        UpdateData(spellData);
        UpdatePositionAndVideo(position, spellData.type);
    }
    public void ShowPlayerSkillDetails(PlayerSkillData skillData, UIHoverPosition position = null) {
        UpdateData(skillData);
        UpdatePositionAndVideo(position, skillData.skill);
    }
    public void ShowPlayerSkillDetails(string title, string description, int charges = -1, int manaCost = -1, 
        int cooldown = -1, int threat = -1, string additionalText = "", UIHoverPosition position = null) {
        UpdateData(title, description, charges, manaCost, cooldown, threat, additionalText);
        UpdatePositionAndVideo(position, PLAYER_SKILL_TYPE.NONE);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
        tooltipVideoPlayer.Stop();
    }
    private void UpdatePositionAndVideo(UIHoverPosition position, PLAYER_SKILL_TYPE spellType) {
        bool wasActiveBefore = gameObject.activeSelf;
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

        if (wasActiveBefore == false) {
            PLAYER_SKILL_TYPE skillType = spellType;
            PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
            if (data != null) {
                if (data.tooltipImage != null) {
                    tooltipImage.texture = data.tooltipImage;
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 503f);
                    tooltipImage.gameObject.SetActive(true);
                } else if (data.tooltipVideoClip != null && !Settings.SettingsManager.Instance.doNotShowVideos) {
                    tooltipVideoPlayer.clip = data.tooltipVideoClip;
                    tooltipImage.texture = tooltipVideoRenderTexture;
                    tooltipVideoPlayer.Play();
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 503f);
                    tooltipImage.gameObject.SetActive(true);
                } else {
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 264f);
                    tooltipImage.texture = null;
                    tooltipImage.gameObject.SetActive(false);
                }
            } else {
                thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 264f);
                tooltipImage.texture = null;
                tooltipImage.gameObject.SetActive(false);
            }
        }
    }
    private void UpdateData(PlayerSkillData skillData) {
        SkillData spellData = PlayerSkillManager.Instance.GetPlayerSkillData(skillData.skill);
        titleText.SetText(spellData.name);
        descriptionText.SetTextAndReplaceWithIcons(spellData.description);
        int charges =  SpellUtilities.GetModifiedSpellCost(skillData.charges, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetChargeCostsModification());
        int manaCost = SpellUtilities.GetModifiedSpellCost(skillData.manaCost, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCostsModification());
        int cooldown = SpellUtilities.GetModifiedSpellCost(skillData.cooldown, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetCooldownSpeedModification());
        int threat = SpellUtilities.GetModifiedSpellCost(skillData.threat, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetThreatModification());

        string currencyStr = string.Empty; 
        
        if (manaCost > 0) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (charges > 0) {
            //NOTE: Use charges in both max and current amount since PlayerSkillData is just the raw spell data that has not yet been used
            currencyStr += $"{charges.ToString()}/{charges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (cooldown > 0) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (threat > 0) {
            currencyStr += $"{threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        
        currenciesText.text = currencyStr;
        additionalText.text = string.Empty;
    }
    private void UpdateData(SkillData spellData) {
        titleText.text = spellData.name;
        string fullDescription = spellData.description;
        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;

        string currencyStr = string.Empty; 
        
        if (manaCost > 0) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (spellData.maxCharges > 0) {
            currencyStr += $"{charges.ToString()}/{spellData.maxCharges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (cooldown > 0) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (spellData.threat > 0) {
            currencyStr += $"{spellData.threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        currenciesText.text = currencyStr;

        additionalText.text = string.Empty;
        if (spellData is PlayerAction) {
            object activeObj = UIManager.Instance.GetCurrentlySelectedObject();
            if (activeObj != null) {
                if (activeObj is Character activeCharacter) {
                    if (spellData.CanPerformAbilityTowards(activeCharacter) == false) {
                        if (spellData is PlayerAction playerAction && !playerAction.canBeCastOnBlessed && activeCharacter.traitContainer.IsBlessed()) {
                            additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Blessed Villagers are protected from your powers.")}\n";
                        }
                        string wholeReason = spellData
                            .GetReasonsWhyCannotPerformAbilityTowards(activeCharacter);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                } else if (activeObj is TileObject activeTileObject) {
                    if (activeTileObject is AnkhOfAnubis ankh && ankh.isActivated && spellData.type == PLAYER_SKILL_TYPE.SEIZE_OBJECT) {
                        additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Activated Ankh can no longer be seized.")}\n";
                    }
                } else if (activeObj is BaseSettlement activeSettlement) {
                    if (spellData.CanPerformAbilityTowards(activeSettlement) == false) {
                        string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeSettlement);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                } else if (activeObj is LocationStructure activeStructure) {
                    if (spellData.CanPerformAbilityTowards(activeStructure) == false) {
                        string wholeReason = spellData.GetReasonsWhyCannotPerformAbilityTowards(activeStructure);
                        wholeReason = UtilityScripts.Utilities.SplitStringIntoNewLines(wholeReason, ',');
                        additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText(wholeReason)}";
                    }
                }
            }
        }
        
        if (spellData is BrainwashData && UIManager.Instance.structureRoomInfoUI.isShowing && UIManager.Instance.structureRoomInfoUI.activeRoom is DefilerRoom defilerRoom && defilerRoom.charactersInRoom.Count > 0) {
            Character targetCharacter = defilerRoom.charactersInRoom.First();
            if (targetCharacter != null) {
                fullDescription += $"\n<b>{targetCharacter.name} Brainwash Success Rate: {DefilerRoom.GetBrainwashSuccessRate(targetCharacter).ToString("N0")}%</b>";    
            }
        }
        
        descriptionText.SetTextAndReplaceWithIcons(fullDescription);
        
        if(HasEnoughMana(spellData) == false) {
            additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Not enough mana.")}\n";
        }
        if(HasEnoughCharges(spellData) == false) {
            if (spellData.hasCooldown) {
                additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Recharging.")}\n";
            } else {
                additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Not enough charges.")}\n";
            }
        }
    }
    private void UpdateData(string title, string description, int charges, int manaCost, int cooldown, int threat, string additionalTextStr) {
        titleText.text = title;
        descriptionText.SetTextAndReplaceWithIcons(description);
        string currencyStr = string.Empty; 
        
        if (manaCost > 0) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (charges > 0) {
            currencyStr += $"{charges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (cooldown > 0) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (threat > 0) {
            currencyStr += $"{threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        currenciesText.text = currencyStr;
        additionalText.text = additionalTextStr;

        if (manaCost > 0) {
            if(HasEnoughMana(manaCost) == false) {
                additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Not enough mana.")}\n";
            }    
        }

        if (charges != -1) {
            if(HasEnoughCharges(charges) == false) {
                if (cooldown != -1) {
                    additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Recharging.")}\n";
                } else {
                    additionalText.text += $"{UtilityScripts.Utilities.ColorizeInvalidText("Not enough charges.")}\n";
                }
            }    
        }
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
    
}
