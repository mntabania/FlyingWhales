using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using Debug = System.Diagnostics.Debug;

public class PlayerSkillDetailsTooltip : MonoBehaviour {
    public RectTransform thisRect;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
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

    private SpellData skillData;

    public void ShowPlayerSkillDetails(SpellData skillData, UIHoverPosition position = null) {
        this.skillData = skillData;
        UpdateData();
        bool wasActiveBefore = gameObject.activeSelf;
        gameObject.SetActive(true);
        UIHoverPosition positionToUse = position;
        if (positionToUse == null) {
            positionToUse = defaultPosition;
        }

        RectTransform thisRect = transform as RectTransform;
        Debug.Assert(thisRect != null, nameof(thisRect) + " != null");
        thisRect.SetParent(positionToUse.transform);
        
        thisRect.pivot = positionToUse.pivot;
        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.zero;
        UtilityScripts.Utilities.GetAnchorMinMax(positionToUse.anchor, ref anchorMin, ref anchorMax);
        thisRect.anchorMin = anchorMin;
        thisRect.anchorMax = anchorMax;
        thisRect.anchoredPosition = Vector2.zero;

        if (wasActiveBefore == false) {
            PlayerSkillAssets skillAssets = PlayerSkillManager.Instance.GetPlayerSkillAsset(skillData.type);
            if (skillAssets != null) {
                if (skillAssets.tooltipImage != null) {
                    tooltipImage.texture = skillAssets.tooltipImage;
                    thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 727f);
                    tooltipImage.gameObject.SetActive(true);
                } else if (skillAssets.tooltipVideoClip != null) {
                    tooltipVideoPlayer.clip = skillAssets.tooltipVideoClip;
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
    public void HidePlayerSkillDetails() {
        gameObject.SetActive(false);
    }

    private void UpdateData() {
        titleText.text = skillData.name;
        descriptionText.text = skillData.description;
        threatText.text = "" + skillData.threat;
        threatPerHourText.text = "" + skillData.threatPerHour;

        int charges = skillData.charges;
        int manaCost = skillData.manaCost;
        int cooldown = skillData.cooldown;
        categoryText.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(skillData.category.ToString());

        chargesText.text = "N/A";
        if(charges != -1) {
            chargesText.text = $"{charges.ToString()}/{skillData.maxCharges.ToString()}";
        }

        manaCostText.text = "N/A";
        if (manaCost != -1) {
            manaCostText.text = HasEnoughMana() ? "<color=\"green\">" : "<color=\"red\">";
            manaCostText.text += $"{manaCost.ToString()}</color>" ;
        }

        string cdText = cooldown == -1 ? "N/A" : $"{GameManager.GetTimeAsWholeDuration(cooldown).ToString()} {GameManager.GetTimeIdentifierAsWholeDuration(cooldown)}";
        cooldownText.text = cdText;

        additionalText.text = string.Empty;
        if (skillData is PlayerAction && UIManager.Instance.characterInfoUI.isShowing) {
            if (UIManager.Instance.characterInfoUI.activeCharacter.traitContainer.HasTrait("Blessed")) {
                additionalText.text += "<color=\"red\">Blessed Villagers are protected from your powers.</color>\n";    
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
        if (skillData.hasManaCost) {
            if (PlayerManager.Instance.player.mana >= skillData.manaCost) {
                return true;
            }
            return false;
        }
        //if skill has no mana cost then always has enough mana
        return true;
    }
    private bool HasEnoughCharges() {
        if (skillData.hasCharges) {
            if (skillData.charges > 0) {
                return true;
            }
            return false;
        }
        //if skill has no charges then always has enough charges
        return true;
    }
}
