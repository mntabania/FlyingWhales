using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Character_Talents;

public class CharacterTalentTooltip : MonoBehaviour {
    public RectTransform thisRect;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public RuinarchText descriptionText;
    public TextMeshProUGUI experienceText;
    public TextMeshProUGUI bonusesText;
    public UIHoverPosition defaultPosition;

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
        thisRect.sizeDelta = new Vector2(thisRect.sizeDelta.x, 464f);
    }
    private void UpdateData(Character character, CHARACTER_TALENT type) {
        CharacterTalent talent = character.talentComponent.GetTalent(type);
        CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talent.talentType);
        titleText.text = talentData.name;
        levelText.text = "Lv." + talent.level;
        descriptionText.text = talentData.description;
        if (talent.level >= 5) {
            experienceText.gameObject.SetActive(false);
        } else {
            experienceText.gameObject.SetActive(true);
            experienceText.text = talent.experience.ToString() + " xp";
        }
        bonusesText.text = talentData.GetBonusDescription(talent.level);
    }

    public void ShowCharacterTalentData(Character character, CHARACTER_TALENT type, UIHoverPosition position = null) {
        UpdateData(character, type);
        UpdatePosition(position);
    }
}
