using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
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
        Messenger.AddListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
    }

    #region Listeners
    private void OnCameraZoomChanged(Camera camera, float amount) {
        if (camera == InnerMapCameraMove.Instance.camera) {
            UpdateSizeBasedOnZoom();
        }
    }
    private void OnLocationMapClosed(Region location) {
        if (location == bedGO.bedTileObject.currentRegion) {
            HideMarkerNameplate();
        }
    }
    private void OnLocationMapOpened(Region location) {
        if (location == bedGO.bedTileObject.currentRegion) {
            UpdateMarkerNameplate(bedGO.bedTileObject);
        }
    }
    #endregion

    #region Monobehaviours
    private void LateUpdate() {
        Vector3 markerScreenPosition =
            InnerMapCameraMove.Instance.camera.WorldToScreenPoint(bedGO.transform.position);
        markerScreenPosition.z = 0f;
        transform.position = markerScreenPosition;
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        bedGO = null;
        Messenger.RemoveListener<Camera, float>(ControlsSignals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnLocationMapOpened);
        Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnLocationMapClosed);
    }
    #endregion

    #region Utilities
    public void UpdateMarkerNameplate(TileObject bedTileObject) {
        int userCount = bedTileObject.GetUserCount();
        bool showActionIcon = false;
        if (bedGO.bedTileObject.currentRegion == InnerMapManager.Instance.currentlyShowingLocation) {
            if (userCount == 1) {
                showActionIcon = true;
            } else if (userCount == 2) {
                showActionIcon = true;
            }    
        }
        if (showActionIcon) {
            bool shouldShowIcon = true;
            Character user = bedTileObject.GetFirstUser();
            if (user == null) {
                HideMarkerNameplate();
                return;
            }
            ActualGoapNode actionNode = user.currentActionNode;
            if (actionNode != null && (actionNode.actionStatus == ACTION_STATUS.PERFORMING || actionNode.actionStatus == ACTION_STATUS.STARTED)) {
                string actionIconString = actionNode.action.GetActionIconString(actionNode);
                if (actionIconString != GoapActionStateDB.No_Icon) {
                    UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[actionIconString]);
                } else {
                    shouldShowIcon = false;
                }
            } else {
                if (user.traitContainer.HasTrait("Quarantined")) {
                    UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Sick_Icon]);
                } else {
                    UpdateActionIcon(InteractionManager.Instance.actionIconDictionary[GoapActionStateDB.Sleep_Icon]);    
                }
            }
            if (shouldShowIcon) {
                ShowMarkerNameplate();
            } else {
                HideMarkerNameplate();
            }
        } else {
            HideMarkerNameplate();
        }
    }
    public void ShowMarkerNameplate() {
        Assert.IsTrue(bedGO.bedTileObject.currentRegion == InnerMapManager.Instance.currentlyShowingLocation, $"Something is trying to activate action icon of {name} even though its region map is not active!");
        gameObject.SetActive(true);
    }
    public void HideMarkerNameplate() {
        gameObject.SetActive(false);
    }
    private void UpdateActionIcon(Sprite sprite) {
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
