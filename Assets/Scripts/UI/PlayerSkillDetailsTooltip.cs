using System.Collections;
using System.Collections.Generic;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UtilityScripts;
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

    public void ShowPlayerSkillDetails(SpellData spellData, UIHoverPosition position = null) {
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
        UpdatePositionAndVideo(position, SPELL_TYPE.NONE);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
        tooltipVideoPlayer.Stop();
    }
    private void UpdatePositionAndVideo(UIHoverPosition position, SPELL_TYPE spellType) {
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
            SPELL_TYPE skillType = spellType;
            PlayerSkillData data = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
            if (data != null) {
                if (data.tooltipImage != null) {
                    tooltipImage.texture = data.tooltipImage;
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 503f);
                    tooltipImage.gameObject.SetActive(true);
                } else if (data.tooltipVideoClip != null) {
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
        SpellData spellData = PlayerSkillManager.Instance.GetPlayerSpellData(skillData.skill);
        titleText.SetText(spellData.name);
        descriptionText.SetTextAndReplaceWithIcons(spellData.description);
        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;

        string currencyStr = string.Empty; 
        
        if (manaCost != -1) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (cooldown != -1) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (charges != -1) {
            currencyStr += $"{charges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (skillData.threat > 0) {
            currencyStr += $"{skillData.threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        
        currenciesText.text = currencyStr;
        additionalText.text = string.Empty;
    }
    private void UpdateData(SpellData spellData) {
        titleText.text = spellData.name;
        descriptionText.SetTextAndReplaceWithIcons(spellData.description);
        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;

        string currencyStr = string.Empty; 
        
        if (manaCost != -1) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (cooldown != -1) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (charges != -1) {
            currencyStr += $"{charges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (spellData.threat > 0) {
            currencyStr += $"{spellData.threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        currenciesText.text = currencyStr;

        additionalText.text = string.Empty;
        if (spellData is PlayerAction || spellData.category == SPELL_CATEGORY.AFFLICTION) {
            IPointOfInterest activePOI = UIManager.Instance.GetCurrentlySelectedPOI();
            if (activePOI != null) {
                if (activePOI is Character activeCharacter) {
                    if (spellData.CanPerformAbilityTowards(activeCharacter) == false) {
                        if (activeCharacter.traitContainer.HasTrait("Blessed")) {
                            additionalText.text += $"<color=\"red\">Blessed Villagers are protected from your powers.</color>\n";
                        }
                        string wholeReason = spellData
                            .GetReasonsWhyCannotPerformAbilityTowards(activeCharacter);
                        if (string.IsNullOrEmpty(wholeReason) == false) {
                            string[] reasons = wholeReason.Split(',');
                            for (int i = 0; i < reasons.Length; i++) {
                                string reason = reasons[i];
                                additionalText.text += $"<color=\"red\">{reason}</color>\n";
                            }
                        }
                    }
                } else if (activePOI is TileObject activeTileObject) {
                    if (activeTileObject is AnkhOfAnubis ankh && ankh.isActivated && spellData.type == SPELL_TYPE.SEIZE_OBJECT) {
                        additionalText.text += "<color=\"red\">Activated Ankh can no longer be seized.</color>\n";
                    }
                }
            }
        }
        if(HasEnoughMana(spellData) == false) {
            additionalText.text += "<color=\"red\">Not enough mana.</color>\n";
        }
        if(HasEnoughCharges(spellData) == false) {
            if (spellData.hasCooldown) {
                additionalText.text += "<color=\"red\">Recharging.</color>\n";
            } else {
                additionalText.text += "<color=\"red\">Not enough charges.</color>\n";
            }
        }
    }
    private void UpdateData(string title, string description, int charges, int manaCost, int cooldown, int threat, string additionalTextStr) {
        titleText.text = title;
        descriptionText.SetTextAndReplaceWithIcons(description);
        string currencyStr = string.Empty; 
        
        if (manaCost != -1) {
            currencyStr += $"{manaCost.ToString()} {UtilityScripts.Utilities.ManaIcon()}  ";
        }
        if (cooldown != -1) {
            currencyStr += $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)} {UtilityScripts.Utilities.CooldownIcon()}  ";
        }
        if (charges != -1) {
            currencyStr += $"{charges.ToString()} {UtilityScripts.Utilities.ChargesIcon()}  ";
        }
        if (threat > 0) {
            currencyStr += $"{threat.ToString()} {UtilityScripts.Utilities.ThreatIcon()}  ";
        }
        currenciesText.text = currencyStr;
        additionalText.text = additionalTextStr;

        if (manaCost != -1) {
            if(HasEnoughMana(manaCost) == false) {
                additionalText.text += "<color=\"red\">Not enough mana.</color>\n";
            }    
        }

        if (charges != -1) {
            if(HasEnoughCharges(charges) == false) {
                if (cooldown != -1) {
                    additionalText.text += "<color=\"red\">Recharging.</color>\n";
                } else {
                    additionalText.text += "<color=\"red\">Not enough charges.</color>\n";
                }
            }    
        }
    }

    private bool HasEnoughMana(SpellData spellData) {
        if (spellData.hasManaCost) {
            if (PlayerManager.Instance.player.mana >= spellData.manaCost) {
                return true;
            }
            return false;
        }
        //if skill has no mana cost then always has enough mana
        return true;
    }
    private bool HasEnoughCharges(SpellData spellData) {
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
