using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SummonMinionPlayerSkillButton : MonoBehaviour, IPointerClickHandler {
    public SummonMinionPlayerSkillNameplateItem nameplateItem;

    public void OnPointerClick(PointerEventData eventData) {
        nameplateItem.OnPointerClick(eventData);
    }
}
