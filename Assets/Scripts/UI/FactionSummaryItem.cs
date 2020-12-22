using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactionSummaryItem : PooledObject, IPointerClickHandler {

    private Faction _faction;

    [SerializeField] private TextMeshProUGUI factionNameLbl;
    [SerializeField] private Image bg;
    [SerializeField] private FactionEmblem emblem;

    public void SetFaction(Faction faction) {
        _faction = faction;
        factionNameLbl.text = _faction.nameWithColor;
        emblem.SetFaction(_faction);
        //emblemBG.sprite = faction.emblemBG;
    }
    public void SetBGColor(Color color) {
        bg.color = color;
    }
    public void OnPointerClick(PointerEventData eventData) {
        UIManager.Instance.ShowFactionInfo(_faction);
    }

    public override void Reset() {
        base.Reset();
        _faction = null;
    }
}
