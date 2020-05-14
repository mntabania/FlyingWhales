using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Debug = System.Diagnostics.Debug;

public class PlayerSkillDetailsTooltip : MonoBehaviour {
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI chargesText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI threatText;
    public TextMeshProUGUI threatPerHourText;
    public UIHoverPosition defaultPosition;
    
    
    private SpellData skillData;

    public void ShowPlayerSkillDetails(SpellData skillData, UIHoverPosition position = null) {
        this.skillData = skillData;
        UpdateData();
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
        chargesText.text = "" + (charges != -1 ? charges : 0);
        manaCostText.text = "" + (manaCost != -1 ? manaCost : 0);

        string cdText = string.Empty;
        if(cooldown == -1) {
            cdText = "0 mins";
        } else {
            cdText = GameManager.GetTimeAsWholeDuration(cooldown) + " " + GameManager.GetTimeIdentifierAsWholeDuration(cooldown);
        }
        cooldownText.text = cdText;
    }
}
