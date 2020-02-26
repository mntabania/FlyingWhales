using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocationPortrait : PooledObject, IPointerClickHandler {

    public Region region { get; private set; }
    //public BaseLandmark landmark { get; private set; }

    [SerializeField] private Image portrait;
    [SerializeField] private GameObject hoverObj;

    public bool disableInteraction;

    public void OnPointerClick(PointerEventData eventData) {
        if (!disableInteraction && eventData.button == PointerEventData.InputButton.Left) {
            if (region != null) {
                if (!UIManager.Instance.regionInfoUI.isShowing) {
                    UIManager.Instance.ShowRegionInfo(region);
                } else {
                    region.CenterCameraOnRegion();
                }
            } 
            //else if (landmark != null) {
            //    UIManager.Instance.ShowHextileInfo(landmark.tileLocation);
            //}
        }
    }

    public void SetLocation(Region region) {
        this.region = region;
        //this.landmark = null;
        SetPortrait(region.mainLandmark.specificLandmarkType);
        
    }
    public void SetPortrait(LANDMARK_TYPE landmarkType) {
        portrait.sprite = LandmarkManager.Instance.GetLandmarkData(landmarkType).landmarkPortrait;
    }
    //public void SetLocation(BaseLandmark landmark) {
    //    this.landmark = landmark;
    //    this.region = null;
    //    portrait.sprite = LandmarkManager.Instance.GetLandmarkData(landmark.specificLandmarkType).landmarkPortrait;
    //}

    public void SetHoverHighlightState(bool state) {
        if (!disableInteraction) {
            hoverObj.SetActive(state);
        }
    }

    public void ShowLocationInfo() {
        if (region != null) {
            UIManager.Instance.ShowSmallInfo(region.name);
        } 
        //else if (this.landmark != null) {
        //    UIManager.Instance.ShowSmallInfo(this.landmark.tileLocation.region.name);
        //}
        
    }
    public void HideLocationInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        region = null;
        //landmark = null;
    }
}
