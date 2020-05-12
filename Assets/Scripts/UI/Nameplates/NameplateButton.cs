using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NameplateButton : MonoBehaviour, IPointerClickHandler {
    public INameplateItem nameplateItem;

    public void SetNameplateItem(INameplateItem nameplateItem) {
        this.nameplateItem = nameplateItem;
    }
    public void OnPointerClick(PointerEventData eventData) {
        nameplateItem.OnPointerClick(eventData);
    }
}
