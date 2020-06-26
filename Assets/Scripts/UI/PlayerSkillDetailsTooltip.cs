using System.Collections;
using System.Collections.Generic;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using Debug = System.Diagnostics.Debug;

public class PlayerSkillDetailsTooltip : MonoBehaviour {
    public RectTransform thisRect;
    public TextMeshProUGUI titleText;
    public RuinarchText descriptionText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI chargesText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI threatText;
    public TextMeshProUGUI threatPerHourText;
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
        UpdatePlayerSkillDetails(position);
    }
    public void ShowPlayerSkillDetails(PlayerSkillData skillData, UIHoverPosition position = null) {
        this.skillData = skillData;
        UpdateData(skillData);
        UpdatePlayerSkillDetails(position);
    }
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
        tooltipVideoPlayer.Stop();
    }
    private void UpdatePlayerSkillDetails(UIHoverPosition position) {
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
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 727f);
                    tooltipImage.gameObject.SetActive(true);
                } else if (skillData.tooltipVideoClip != null) {
                    tooltipVideoPlayer.clip = skillData.tooltipVideoClip;
                    tooltipImage.texture = tooltipVideoRenderTexture;
                    tooltipVideoPlayer.Play();
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 727f);
                    tooltipImage.gameObject.SetActive(true);
                } else {
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 494f);
                    tooltipImage.texture = null;
                    tooltipImage.gameObject.SetActive(false);
                }
            } else {
                thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 494f);
                tooltipImage.texture = null;
                tooltipImage.gameObject.SetActive(false);
            }
        }
    }
    private void UpdateData(PlayerSkillData skillData) {
        titleText.SetText(skillData.name);
        descriptionText.SetText(PlayerSkillManager.Instance.GetPlayerSpellData(skillData.skill).description);
        threatText.SetText("" + skillData.threat);
        threatPerHourText.SetText("" + skillData.threatPerHour);

        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;
        categoryText.SetText(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(PlayerSkillManager.Instance.GetPlayerSpellData(skillData.skill).category.ToString()));

        chargesText.SetText("N/A");
        if (charges != -1) {
            chargesText.SetText($"{charges.ToString()}");
        }

        manaCostText.SetText("N/A");
        if (manaCost != -1) {
            manaCostText.text += $"{manaCost.ToString()}";
        }

        string cdText = cooldown == -1 ? "N/A" : $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)}";
        cooldownText.SetText(cdText);

    }
    private void UpdateData(SpellData spellData) {
        titleText.SetText(spellData.name);
        descriptionText.SetText(spellData.description);
        threatText.SetText("" + spellData.threat);
        threatPerHourText.SetText("" + spellData.threatPerHour);

        int charges = spellData.charges;
        int manaCost = spellData.manaCost;
        int cooldown = spellData.cooldown;
        categoryText.SetText(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spellData.category.ToString()));

        chargesText.SetText("N/A");
        if(charges != -1) {
            chargesText.SetText($"{charges.ToString()}/{spellData.maxCharges.ToString()}");
        }

        manaCostText.SetText("N/A");
        if (manaCost != -1) {
            manaCostText.SetText(HasEnoughMana() ? "<color=\"green\">" : "<color=\"red\">");
            manaCostText.text += $"{manaCost.ToString()}</color>" ;
        }

        string cdText = cooldown == -1 ? "N/A" : $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)}";
        cooldownText.SetText(cdText);

        additionalText.SetText(string.Empty);
        if (UIManager.Instance.characterInfoUI.isShowing) {
            if (UIManager.Instance.characterInfoUI.activeCharacter.traitContainer.HasTrait("Blessed")) {
                additionalText.text += $"<color=\"red\">Blessed Villagers are protected from your powers.</color>\n";    
            }
            if (spellData.CanPerformAbilityTowards(UIManager.Instance.characterInfoUI.activeCharacter) == false) {
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
