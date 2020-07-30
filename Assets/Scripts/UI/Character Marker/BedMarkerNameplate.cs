using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BedMarkerNameplate : PooledObject {

    [SerializeField] private RectTransform thisRect;
    [SerializeField] private GameObject visualsParent;
    [SerializeField] private Image actionIcon;
    
    private BedObjectGameObject bedGO;

    private const float DefaultSize = 80f;

    public void Initialize(BedObjectGameObject bedGO) {
        name = $"{bedGO.name} Marker Nameplate";
        this.bedGO = bedGO;
        UpdateSizeBasedOnZoom();
        Messenger.AddListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }

    #region Listeners
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.innerMapsCamera) {
            UpdateSizeBasedOnZoom();
        }
    }
    #endregion

    #region Monobehaviours
    private void LateUpdate() {
        Vector3 markerScreenPosition =
            InnerMapCameraMove.Instance.innerMapsCamera.WorldToScreenPoint(bedGO.transform.position);
        markerScreenPosition.z = 0f;
        transform.position = markerScreenPosition;
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        bedGO = null;
        Messenger.RemoveListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }
    #endregion

    #region Utilities
    public void ShowMarkerNameplate() {
        gameObject.SetActive(true);
    }
    public void HideMarkerNameplate() {
        gameObject.SetActive(false);
    }
    public void UpdateActionIcon(Sprite sprite) {
        actionIcon.sprite = sprite;
    }
    private void UpdateSizeBasedOnZoom() {
        float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
        float spriteSize = bedGO.defaultBed.rect.width;
        spriteSize += (4f * fovDiff);
        float size = spriteSize; //- (fovDiff * 4f);
        thisRect.sizeDelta = new Vector2(size, size);

        //float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
        //float spriteSize = bed.character.visuals.selectableSize.y * 100f;
        //if (bed.character.grave != null) {
        //    spriteSize = DefaultSize - (fovDiff * 4f);
        //} else {
        //    if (bed.character is Dragon) {
        //        spriteSize -= (12f * fovDiff);  
        //    } else {
        //        spriteSize -= (4f * fovDiff);
        //    }
        //}
        //float size = spriteSize; //- (fovDiff * 4f);
        //thisRect.sizeDelta = new Vector2(size, size);
    }
    #endregion
}
