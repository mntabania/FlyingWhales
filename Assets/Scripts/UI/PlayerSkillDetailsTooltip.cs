using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSkillDetailsTooltip : MonoBehaviour {
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI chargesText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI threatText;
    public TextMeshProUGUI threatPerHourText;

    private SpellData skillData;

    public void ShowPlayerSkillDetails(SpellData skillData) {
        this.skillData = skillData;
        UpdateData();
        gameObject.SetActive(true);
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
