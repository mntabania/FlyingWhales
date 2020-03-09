using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreaPickerItem : ObjectPickerItem<NPCSettlement>, IPointerClickHandler {

    public Action<NPCSettlement> onClickAction;

    private NPCSettlement _npcSettlement;

    [SerializeField] private LocationPortrait portrait;
    public GameObject portraitCover;

    public override NPCSettlement obj { get { return _npcSettlement; } }

    public void SetArea(NPCSettlement npcSettlement) {
        this._npcSettlement = npcSettlement;
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        portrait.SetLocation(_npcSettlement.region);
        mainLbl.text = _npcSettlement.name;
        //subLbl.text = Utilities.GetNormalizedSingularRace(npcSettlement.race) + " " + npcSettlement.characterClass.className;
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(_npcSettlement);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            //portrait.OnClick();
            UIManager.Instance.ShowRegionInfo(_npcSettlement.region);
        } else {
            OnClick();
        }
    }
}
