using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegionNameplateItem : NameplateItem<Region> {

    [Header("Region Attributes")]
    [SerializeField] private LocationPortrait portrait;
    //public GameObject portraitCover;

    private Region region;
    public override Region obj { get { return region; } }

    public override void SetObject(Region o) {
        base.SetObject(o);
        this.region = o;
        UpdateVisuals();
    }

    //public override void SetButtonState(bool state) {
    //    base.SetButtonState(state);
    //    portraitCover.SetActive(!state);
    //}

    private void UpdateVisuals() {
        portrait.SetLocation(region);
        mainLbl.text = region.name;
        subLbl.text = string.Empty;
    }
}

