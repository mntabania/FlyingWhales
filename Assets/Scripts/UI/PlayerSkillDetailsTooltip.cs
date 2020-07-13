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

    private SpellData spellData;
    private PlayerSkillData skillData;

    public void ShowPlayerSkillDetails(SpellData spellData, UIHoverPosition position = null) {
        this.spellData = spellData;
        UpdateData(spellData);
        UpdatePositionAndVideo(position);
    }
    public void ShowPlayerSkillDetails(PlayerSkillData skillData, UIHoverPosition position = null) {
        this.skillData = skillData;
        UpdateData(skillData);
        UpdatePositionAndVideo(position);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
        tooltipVideoPlayer.Stop();
    }
    private void UpdatePositionAndVideo(UIHoverPosition position) {
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
            SPELL_TYPE skillType = SPELL_TYPE.NONE;
            if(spellData != null) {
                skillType = spellData.type;
            } else if (this.skillData != null) {
                skillType = this.skillData.skill;
            }
            PlayerSkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData<PlayerSkillData>(skillType);
            if (skillData != null) {
                if (skillData.tooltipImage != null) {
                    tooltipImage.texture = skillData.tooltipImage;
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 503f);
                    tooltipImage.gameObject.SetActive(true);
                } else if (skillData.tooltipVideoClip != null) {
                    tooltipVideoPlayer.clip = skillData.tooltipVideoClip;
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
        titleText.SetText(skillData.name);
        descriptionText.SetText(PlayerSkillManager.Instance.GetPlayerSpellData(skillData.skill).description);
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
        descriptionText.SetText(spellData.description);
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
        if (UIManager.Instance.characterInfoUI.isShowing && spellData is PlayerAction) {
            if (spellData.CanPerformAbilityTowards(UIManager.Instance.characterInfoUI.activeCharacter) == false) {
                if (UIManager.Instance.characterInfoUI.activeCharacter.traitContainer.HasTrait("Blessed")) {
                    additionalText.text += $"<color=\"red\">Blessed Villagers are protected from your powers.</color>\n";    
                }
                string wholeReason = spellData
                    .GetReasonsWhyCannotPerformAbilityTowards(UIManager.Instance.characterInfoUI.activeCharacter);
                if (string.IsNullOrEmpty(wholeReason) == false) {
                    string[] reasons = wholeReason.Split(',');
                    for (int i = 0; i < reasons.Length; i++) {
                        string reason = reasons[i];
                        additionalText.text += $"<color=\"red\">{reason}</color>\n";   
                    }
                }
                
            }
        }
        if(HasEnoughMana() == false) {
            additionalText.text += "<color=\"red\">Not enough mana.</color>\n";
        }
        if(HasEnoughCharges() == false) {
            additionalText.text += "<color=\"red\">Not enough charges.</color>\n";
        }
    }

    private bool HasEnoughMana() {
        if (spellData.hasManaCost) {
            if (PlayerManager.Instance.player.mana >= spellData.manaCost) {
                return true;
            }
            return false;
        }
        //if skill has no mana cost then always has enough mana
        return true;
    }
    private bool HasEnoughCharges() {
        if (spellData.hasCharges) {
            if (spellData.charges > 0) {
                return true;
            }
            return false;
        }
        //if skill has no charges then always has enough charges
        return true;
    }
}
