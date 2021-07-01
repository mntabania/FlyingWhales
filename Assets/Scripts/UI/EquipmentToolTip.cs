using UnityEngine;
using TMPro;
using Character_Talents;

public class EquipmentToolTip : MonoBehaviour {
    public RectTransform thisRect;
    public TextMeshProUGUI titleText;
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
    private void UpdateData(EquipmentItem p_item) {
        
        titleText.text = p_item.name;
        bonusesText.text = p_item.GetBonusDescription();
    }

    public void ShowEquipmentItem(EquipmentItem p_item, UIHoverPosition position = null) {
        UpdateData(p_item);
        UpdatePosition(position);
    }
}
