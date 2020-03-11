using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnsummonedMinionNameplateItem : NameplateItem<UnsummonedMinionData> {

    [Header("Unsummon Minion Nameplate Attributes")]
    [SerializeField] private Image classPortrait;
    [SerializeField] private GameObject hoverPortrait;

    public UnsummonedMinionData minionData { get; private set; }

    #region Overrides
    public override void SetObject(UnsummonedMinionData o) {
        base.SetObject(o);
        minionData = o;
        mainLbl.text = o.minionName;
        subLbl.text = $"Demon {o.className}";
        Sprite classPortrait = CharacterManager.Instance.GetWholeImagePortraitSprite(minionData.className);
        this.classPortrait.sprite = classPortrait;
    }
    public override void OnHoverEnter() {
        hoverPortrait.SetActive(true);
        //UIManager.Instance.ShowMinionCardTooltip(minionData);
        base.OnHoverEnter();
    }
    public override void OnHoverExit() {
        hoverPortrait.SetActive(false);
        //UIManager.Instance.HideMinionCardTooltip();
        base.OnHoverExit();
    }
    #endregion
}
