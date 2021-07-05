using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TileObjectNameplateItem : NameplateItem<TileObject> {

    [Header("Basic Data")]
    [SerializeField] private TileObjectPortrait tileObjectPortrait;

    public override void SetObject(TileObject o) {
        base.SetObject(o);
        tileObjectPortrait.SetTileObject(o);
        tileObjectPortrait.SetRightClickAction(OnRightClickPortrait);
        UpdateBasicData();
    }
    private void UpdateBasicData() {
        mainLbl.text = obj.name;
    }
    private void OnRightClickPortrait(TileObject p_tileObject) {
        UIManager.Instance.ShowPlayerActionContextMenu(p_tileObject, Input.mousePosition, true);
    }
    public void SetPosition(UIHoverPosition position) {
        UIManager.Instance.PositionTooltip(position, gameObject, this.transform as RectTransform);
    }
}

