using UnityEngine;
using System.Collections;
using System.Linq;
using Inner_Maps;
using Ruinarch;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using DG.Tweening;
using Inner_Maps.Location_Structures;
using Settings;

public class InnerMapCameraMove : BaseCameraMove {

	public static InnerMapCameraMove Instance;
    
	[SerializeField] private float zoomSensitivity;
    //[FormerlySerializedAs("areaMapsCamera")] public Camera camera;
    [SerializeField] private bool allowZoom = true;

    [Header("Shaking")]
    [SerializeField] private RFX4_CameraShake cameraShake;

    private float previousCameraFOV;

    [SerializeField] private bool cameraControlEnabled = false;
    [SerializeField] private float xSeeLimit;

    public Tweener innerMapCameraShakeMeteorTween { get; private set; }

    #region getters/setters
    public float currentFOV => camera.orthographicSize;
    public float maxFOV => _maxFov;
    public float minFOV => _minFov;
    #endregion

    private void Awake(){
		Instance = this;
	}
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    private void Update() {
        if (!cameraControlEnabled) {
            return;
        }
        ArrowKeysMovement();
        Dragging(camera);
        Edging();
        Zooming();
        Targeting(camera);
        ConstrainCameraBounds(camera);
    }

    public override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
        
    }

    #region Listeners
    private void OnInnerMapOpened(Region location) {
        gameObject.SetActive(true);
        SetCameraControlState(true);
        SetCameraBordersForMap(location.innerMap);
        ConstrainCameraBounds(camera);
        camera.depth = 2;
        // AudioManager.Instance.SetCameraParent(this);
    }
    private void OnInnerMapClosed(Region location) {
        // AudioManager.Instance.SetCameraParent(WorldMapCameraMove.Instance);
        gameObject.SetActive(false);
        SetCameraControlState(false);
        camera.depth = 0;
    }
    #endregion

    #region Positioning
    public void MoveCamera(Vector3 newPos) {
        transform.position = newPos;
        //ConstrainCameraBounds();
    }
    public void JustCenterCamera(bool instantCenter) {
        if (instantCenter) {
            Vector3 center = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
            MoveCamera(center);
        } else {
            InnerMapManager.Instance.currentlyShowingMap.centerGo.transform.position = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
            target = InnerMapManager.Instance.currentlyShowingMap.centerGo.transform;
        }
    }
    public void CenterCameraOn(GameObject GO, bool instantCenter = false) {
        if (ReferenceEquals(GO, null)) {
            target = null;
        } else {
            if (instantCenter) {
                MoveCamera(GO.transform.position);
            } 
            target = GO.transform;
        }
    }
    public void CenterCameraOnTile(Area area, bool instantCenter = true) {
        if (instantCenter) {
            MoveCamera(area.worldPosition);
        } else {
            target = area.areaItem.transform;    
        }
    }
    private void Zooming() {
        if (!allowZoom) { return; }
        if (!CanMoveCamera()) { return; }
        if (InputManager.Instance.HasSelectedUIObject()) { return; }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) {
            float axis = -0.1f;
            if (Input.GetKeyDown(KeyCode.Z)) {
                axis = 0.1f;
            }
            float fov = camera.orthographicSize;
            float adjustment = axis * (zoomSensitivity);
            if (adjustment != 0f) {
                fov -= adjustment;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);
                camera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(camera, adjustment));
            }
        } else {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (screenRect.Contains(Input.mousePosition)) {
                //camera scrolling code
                float fov = camera.orthographicSize;
                float axis = Input.GetAxis("Mouse ScrollWheel");
                float adjustment = axis * (zoomSensitivity);
                if (adjustment != 0f && !UIManager.Instance.IsMouseOnUI()) {
                    //Debug.Log(adjustment);
                    fov -= adjustment;
                    //fov = Mathf.Round(fov * 100f) / 100f;
                    fov = Mathf.Clamp(fov, _minFov, _maxFov);

                    camera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(camera, adjustment));

                    //if (!Mathf.Approximately(previousCameraFOV, fov)) {
                    //    previousCameraFOV = fov;
                    //    innerMapsCamera.orthographicSize = Mathf.Lerp(innerMapsCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
                    //} else {
                    //    innerMapsCamera.orthographicSize = fov;
                    //}
                    // CalculateCameraBounds();
                
                }
            }   
        }
    }
    public void SetZoom(float p_zoom) {
        float fov = p_zoom;
        fov = Mathf.Clamp(fov, _minFov, _maxFov);
        camera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(camera, p_zoom));
    }
    public void SetCameraBordersForMap(InnerTileMap map) {
        float y = map.transform.localPosition.y;
        //MIN_Y = y;
        //MAX_Y = y;

        MIN_X = map.cameraBounds.x;
        MIN_Y = y + map.cameraBounds.y; //need to offset y values based on position of map, because maps are ordered vertically
        MAX_X = map.cameraBounds.z;
        MAX_Y = y + map.cameraBounds.w;

    }
    #endregion

    #region Bounds
    public bool CanSee(LocationGridTile gridTile) {
        Vector3 viewPos = camera.WorldToViewportPoint(gridTile.centeredWorldLocation);
        return viewPos.x >= 0 && viewPos.x <= xSeeLimit && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z >= 0;
    }
    public bool CanSee(DemonicStructure demonicStructure) {
        if (demonicStructure.structureObj == null) {
            //if ever demonic structures' object has been destroyed, then return false.
            return false;
        }
        Vector3 viewPos = camera.WorldToViewportPoint(demonicStructure.structureObj.transform.position);
        return viewPos.x >= 0 && viewPos.x <= xSeeLimit && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z >= 0;
    }
    #endregion

    #region Camera Control
    private void SetCameraControlState(bool state) {
        cameraControlEnabled = state;
    }
    #endregion

    #region Meteor
    public void MeteorShake() {
        if (SettingsManager.Instance.settings.disableCameraShake) {
            return;
        }
        if (!DOTween.IsTweening(camera)) {
            innerMapCameraShakeMeteorTween = camera.DOShakeRotation(0.8f, new Vector3(8f, 8f, 0f), 35, fadeOut: false);
            innerMapCameraShakeMeteorTween.OnComplete(OnCompleteMeteorShakeTween);
        }    
    }
    private void OnCompleteMeteorShakeTween() {
        camera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
        innerMapCameraShakeMeteorTween = null;
    }
    #endregion

    #region Eartquake
    public void EarthquakeShake() {
        if (SettingsManager.Instance.settings.disableCameraShake) {
            return;
        }
        camera.DOShakeRotation(1f, new Vector3(2f, 2f, 2f), 15, fadeOut: false);
    }
    #endregion
}
