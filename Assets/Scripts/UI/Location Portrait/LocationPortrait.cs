using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : PooledObject, IPointerClickHandler {

    public Region region { get; private set; }
    private BaseSettlement _settlement;

    [SerializeField] private Image portrait;
    [SerializeField] private GameObject hoverObj;

    public bool disableInteraction;

    public void OnPointerClick(PointerEventData eventData) {
        if (!disableInteraction) {
            if (eventData.button == PointerEventData.InputButton.Right) {
                if (_settlement != null) {
                    UIManager.Instance.ShowPlayerActionContextMenu(_settlement, Input.mousePosition, true);
                }
            }
        }
    }

    public void SetLocation(Region region) {
        this.region = region;
    }
    public void SetLocation(BaseSettlement p_settlement) {
        region = null;
        _settlement = p_settlement;
    }
    public void ClearLocations() {
        region = null;
        _settlement = null;
    }
    public void SetPortrait(STRUCTURE_TYPE landmarkType) {
        portrait.sprite = LandmarkManager.Instance.GetStructureData(landmarkType).structureSprite;
    }
    public void SetHoverHighlightState(bool state) {
        if (!disableInteraction) {
            hoverObj.SetActive(state);
        }
    }

    public void ShowLocationInfo() {
        if (region != null) {
            UIManager.Instance.ShowSmallInfo(region.name);
        }
    }
    public void HideLocationInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        region = null;
        _settlement = null;
        //landmark = null;
    }
}
