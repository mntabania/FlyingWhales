using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ApplyTooltipUIMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [FormerlySerializedAs("uiParent")] public InfoUIBase infoUiParent;
    public GameObject objectToCheck;

    private bool isHovering = false;
    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        UIManager.Instance.HideSmallInfo();
    }

    void Update() {
        if (isHovering) {
            infoUiParent.ShowTooltip(objectToCheck);
        }
    }
}
