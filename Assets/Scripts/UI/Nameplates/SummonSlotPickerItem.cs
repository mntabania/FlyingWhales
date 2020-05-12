using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SummonSlotPickerItem : NameplateItem<SummonSlot> {

    [Header("Summon Slot Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    //[SerializeField] private GameObject portraitCover;


    private SummonSlot summonSlot;
    public override SummonSlot obj { get { return summonSlot; } }

    public override void SetObject(SummonSlot o) {
        base.SetObject(o);
        this.summonSlot = o;
        UpdateVisuals();
    }

    //public override void SetButtonState(bool state) {
    //    base.SetButtonState(state);
    //    portraitCover.SetActive(!state);
    //}

    private void UpdateVisuals() {
        portrait.GeneratePortrait(summonSlot.summon);
        mainLbl.text = summonSlot.summon.name;
        subLbl.text = summonSlot.summon.summonType.SummonName();
    }

    //public void OnPointerClick(PointerEventData eventData) {
    //    if (eventData.button == PointerEventData.InputButton.Right) {
    //        //Debug.Log("Right clicked character portrait!");
    //        portrait.OnClick();
    //    } else {
    //        OnClick();
    //    }
    //}
}
