using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionNameplateItem : NameplateItem<Faction> {

    [Header("General")]
    [SerializeField] private Image emblemImg;
    //[SerializeField] private GameObject hoverPortrait;

    public Faction factionData { get; private set; }

    #region Overrides
    public override void SetObject(Faction o) {
        base.SetObject(o);
        factionData = o;
        mainLbl.text = o.nameWithColor;
        subLbl.text = o.factionType.name;
        emblemImg.sprite = o.emblem;
    }
    //public override void OnHoverEnter() {
    //    hoverPortrait.SetActive(true);
    //    //UIManager.Instance.ShowMinionCardTooltip(minionData);
    //    base.OnHoverEnter();
    //}
    //public override void OnHoverExit() {
    //    hoverPortrait.SetActive(false);
    //    //UIManager.Instance.HideMinionCardTooltip();
    //    base.OnHoverExit();
    //}
    public override void Reset() {
        base.Reset();
        factionData = null;
    }
    #endregion
}
