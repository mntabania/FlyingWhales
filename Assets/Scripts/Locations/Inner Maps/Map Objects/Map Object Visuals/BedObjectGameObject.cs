using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BedObjectGameObject : TileObjectGameObject {

    public Sprite defaultBed;
    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public BedMarkerNameplate nameplate { get; private set; }

    public TileObject bedTileObject => obj;

    public override void Initialize(TileObject tileObject) {
        CreateNameplate();
        base.Initialize(tileObject);
        //UpdateSizeBasedOnZoom();
        //Messenger.AddListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }
    public override void Reset() {
        base.Reset();
        ObjectPoolManager.Instance.DestroyObject(nameplate);
        nameplate = null;
        //Messenger.RemoveListener<Camera, float>(Signals.CAMERA_ZOOM_CHANGED, OnCameraZoomChanged);
    }

    public override void UpdateTileObjectVisual(TileObject bed) {
        int userCount = bed.GetUserCount();
        if (userCount == 0) {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(bed, 
                bed.state,
                bed.gridTileLocation.mainBiomeType,
                bed.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
        } else if (userCount == 1) {
            SetVisual(bed1Sleeping);
        } else if (userCount == 2) {
            SetVisual(bed2Sleeping);
        }
        nameplate.UpdateMarkerNameplate(bed);
    }
    
    public override Sprite GetSeizeSprite(IPointOfInterest poi) {
        return defaultBed;
    }
    private void CreateNameplate() {
        GameObject nameplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("BedMarkerNameplate", transform.position,
            Quaternion.identity, UIManager.Instance.characterMarkerNameplateParent);
        nameplate = nameplateGO.GetComponent<BedMarkerNameplate>();
        nameplate.Initialize(this);
    }
    //private void OnCameraZoomChanged(Camera camera, float amount) {
    //    if (camera == InnerMapCameraMove.Instance.innerMapsCamera) {
    //        UpdateSizeBasedOnZoom();
    //    }
    //}
    //private void UpdateSizeBasedOnZoom() {
    //    float fovDiff = InnerMapCameraMove.Instance.currentFOV - InnerMapCameraMove.Instance.minFOV;
    //    float spriteSize = defaultBed.rect.width;
    //    spriteSize += (4f * fovDiff);
    //    float size = spriteSize; //- (fovDiff * 4f);
    //    actionIconRect.sizeDelta = new Vector2(size, size);
    //}
}
