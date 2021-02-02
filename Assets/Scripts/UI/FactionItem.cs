﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class FactionItem : PooledObject {
    public GameObject selectedGO;
    public Image emblem;
    public TextMeshProUGUI nameLbl;
    public TextMeshProUGUI typeLbl;

    public Material grayscaleMat;
    private Color origColorName;
    private Color origColorType = new Color(206f/255f, 182f/255f, 124f/255f);

    public Faction faction { get; private set; }

    private bool isInGrayscale;

    public void SetFaction(Faction faction) {
        origColorName = FactionManager.Instance.factionNameColor;
        this.faction = faction;
        emblem.sprite = faction.emblem;
        nameLbl.text = faction.name;
        nameLbl.text += " (" + faction.GetAliveMembersCount().ToString() + ")";
        typeLbl.text = faction.factionType.name;
    }
    public void UpdateFaction() {
        if(faction != null) {
            emblem.sprite = faction.emblem;
            nameLbl.text = faction.name;
            nameLbl.text += " (" + faction.GetAliveMembersCount().ToString() + ")";
            typeLbl.text = faction.factionType.name;
        }
    }
    public void SetSelected(bool state) {
        selectedGO.SetActive(state);
        SetGrayscale(!state);
    }
    private void SetGrayscale(bool state) {
        isInGrayscale = state;
        if (isInGrayscale) {
            emblem.material = grayscaleMat;
            nameLbl.color = Color.gray;
            typeLbl.color = Color.gray;
        } else {
            emblem.material = null;
            nameLbl.color = origColorName;
            typeLbl.color = origColorType;
        }
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        faction = null;
        selectedGO.SetActive(false);
        isInGrayscale = false;
        emblem.material = null;
        nameLbl.color = origColorName;
        typeLbl.color = origColorType;
    }
    #endregion
}
