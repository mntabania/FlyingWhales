using System;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class WorldMapCameraMove : BaseCameraMove {

	public static WorldMapCameraMove Instance;
    [SerializeField] private Physics2DRaycaster _raycaster;

    [Header("Zooming")]
    [SerializeField] private bool allowZoom = true;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float sensitivity;

    //private properties
    private float previousCameraFOV;
    private bool cameraControlEnabled = true;
    private int defaultMask;
    private Transform _mainCameraTransform;

    public Camera mainCamera { get; private set; }

    private void Awake(){
		Instance = this;
        mainCamera = Camera.main;
        _mainCameraTransform = mainCamera.transform;
        defaultMask = mainCamera.cullingMask;
    }
    private void Update() {
        if (!cameraControlEnabled) {
            return;
        }
        ArrowKeysMovement();
        Dragging(mainCamera);
        Edging();
        Zooming();
        Targeting(mainCamera);
        ConstrainCameraBounds(mainCamera);
    }
    private void OnDestroy() {
        RemoveListeners();
    }

    public override void Initialize() {
        base.Initialize();
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
    }

    private void RemoveListeners() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.RemoveListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
    }

    #region Utilities
    public void ToggleMainCameraLayer(string layerName) {
        int cullingMask = mainCamera.cullingMask;
        cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
        mainCamera.cullingMask = cullingMask;
        defaultMask = cullingMask;
    }
    #endregion

    #region Positioning
    private void OnGameLoaded() {
        Vector3 initialPos = new Vector3(-2.35f, -1.02f, -10f);
        this.transform.position = initialPos;
        _raycaster.enabled = true;
        CalculateCameraBounds(mainCamera);
    }
    private void Zooming() {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (allowZoom && screenRect.Contains(Input.mousePosition)) {
            //camera scrolling code
            float fov = mainCamera.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (sensitivity);
            if (Math.Abs(adjustment) > 0.1f && !UIManager.Instance.IsMouseOnUI()) {
                fov -= adjustment;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);

                mainCamera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(mainCamera, adjustment));
                //if (!Mathf.Approximately(previousCameraFOV, fov)) {
                //    previousCameraFOV = fov;
                //    mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
                //} else {
                //    mainCamera.orthographicSize = fov;
                //}
                CalculateCameraBounds(mainCamera);
                Messenger.Broadcast(Signals.ZOOM_WORLD_MAP_CAMERA, mainCamera);
            }
        }
    }
    #endregion

    

    #region Listeners
    private void OnMenuOpened(InfoUIBase openedBase) { }
    private void OnMenuClosed(InfoUIBase openedBase) { }
    private void OnInnerMapOpened(Region location) {
        // _mainCamera.cullingMask = 0;
        _raycaster.enabled = false;
        gameObject.SetActive(false);
        SetCameraControlState(false);
    }
    private void OnInnerMapClosed(Region location) {
        // _mainCamera.cullingMask = defaultMask;
        _raycaster.enabled = true;
        gameObject.SetActive(true);
        SetCameraControlState(true);
    }
    #endregion

    #region Camera Control
    private void SetCameraControlState(bool state) {
        cameraControlEnabled = state;
    }
    #endregion
}
