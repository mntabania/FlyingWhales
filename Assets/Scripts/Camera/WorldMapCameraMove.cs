// using System;
// using Ruinarch;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using DG.Tweening;
//
// public class WorldMapCameraMove : BaseCameraMove {
//
// 	public static WorldMapCameraMove Instance;
//     [SerializeField] private Physics2DRaycaster _raycaster;
//
//     [Header("Zooming")]
//     [SerializeField] private bool allowZoom = true;
//     [SerializeField] private float _zoomSpeed = 5f;
//     [SerializeField] private float sensitivity;
//
//     //private properties
//     private float previousCameraFOV;
//     private bool cameraControlEnabled = true;
//     private int defaultMask;
//     private Transform _mainCameraTransform;
//
//     public Camera mainCamera { get; private set; }
//
//     private void Awake(){
// 		Instance = this;
//         mainCamera = Camera.main;
//         _mainCameraTransform = mainCamera.transform;
//         defaultMask = mainCamera.cullingMask;
//     }
//     protected override void OnDestroy() {
//         RemoveListeners();
//         base.OnDestroy();
//         Instance = null;
//     }
//     private void Update() {
//         if (!cameraControlEnabled) {
//             return;
//         }
//         ArrowKeysMovement();
//         Dragging(mainCamera);
//         Edging();
//         Zooming();
//         Targeting(mainCamera);
//         ConstrainCameraBounds(mainCamera);
//     }
//
//     public override void Initialize() {
//         base.Initialize();
//         Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
//         Messenger.AddListener<InfoUIBase>(UISignals.MENU_OPENED, OnMenuOpened);
//         Messenger.AddListener<InfoUIBase>(UISignals.MENU_CLOSED, OnMenuClosed);
//         Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
//         Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
//     }
//
//     private void RemoveListeners() {
//         Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
//         Messenger.RemoveListener<InfoUIBase>(UISignals.MENU_OPENED, OnMenuOpened);
//         Messenger.RemoveListener<InfoUIBase>(UISignals.MENU_CLOSED, OnMenuClosed);
//         Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
//         Messenger.RemoveListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
//     }
//
//     #region Utilities
//     public void ToggleMainCameraLayer(string layerName) {
//         int cullingMask = mainCamera.cullingMask;
//         cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
//         mainCamera.cullingMask = cullingMask;
//         defaultMask = cullingMask;
//     }
//     #endregion
//
//     #region Positioning
//     private void OnGameLoaded() {
//         Vector3 initialPos = new Vector3(-2.35f, -1.02f, -10f);
//         this.transform.position = initialPos;
//         _raycaster.enabled = true;
//         CalculateCameraBounds(mainCamera);
//     }
// protected void CalculateCameraBounds(Camera camera) {
//     if (GridMap.Instance.map == null) {
//         return;
//     }
//     Area topRightTile = GridMap.Instance.map[GridMap.Instance.width - 1, GridMap.Instance.height - 1];
//     // Vector3 topRightTilePosition = topRightTile.transform.position;
//         
//     Bounds newBounds = new Bounds {
//         extents = new Vector3(Mathf.Abs(topRightTilePosition.x),
//             Mathf.Abs(topRightTilePosition.y), 0f)
//     };
//     SetCameraBounds(newBounds);
// }
//     private void Zooming() {
//         if (!allowZoom) { return; }
//         if (!CanMoveCamera()) { return; }
//         if (InputManager.Instance.HasSelectedUIObject()) { return; }
//         if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) {
//             float axis = -0.1f;
//             if (Input.GetKeyDown(KeyCode.Z)) {
//                 axis = 0.1f;
//             }
//             float fov = mainCamera.orthographicSize;
//             float adjustment = axis * (sensitivity);
//             if (adjustment != 0f) {
//                 fov -= adjustment;
//                 fov = Mathf.Clamp(fov, _minFov, _maxFov);
//                 mainCamera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(mainCamera, adjustment));
//                 Messenger.Broadcast(ControlsSignals.ZOOM_WORLD_MAP_CAMERA, mainCamera);
//             }
//         } else {
//             Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
//             if (screenRect.Contains(Input.mousePosition)) {
//                 //camera scrolling code
//                 float fov = mainCamera.orthographicSize;
//                 float axis = Input.GetAxis("Mouse ScrollWheel");
//                 float adjustment = axis * (sensitivity);
//                 if (adjustment != 0f && !UIManager.Instance.IsMouseOnUI()) {
//                     //Debug.Log(adjustment);
//                     fov -= adjustment;
//                     //fov = Mathf.Round(fov * 100f) / 100f;
//                     fov = Mathf.Clamp(fov, _minFov, _maxFov);
//
//                     mainCamera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(mainCamera, adjustment));
//
//                     //if (!Mathf.Approximately(previousCameraFOV, fov)) {
//                     //    previousCameraFOV = fov;
//                     //    innerMapsCamera.orthographicSize = Mathf.Lerp(innerMapsCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
//                     //} else {
//                     //    innerMapsCamera.orthographicSize = fov;
//                     //}
//                     // CalculateCameraBounds();
//                     Messenger.Broadcast(ControlsSignals.ZOOM_WORLD_MAP_CAMERA, mainCamera);
//                 }
//             }   
//         }
//         // Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
//         // if (allowZoom && screenRect.Contains(Input.mousePosition)) {
//         //     //camera scrolling code
//         //     float fov = mainCamera.orthographicSize;
//         //     float adjustment = Input.GetAxis("Mouse ScrollWheel") * (sensitivity);
//         //     if (Math.Abs(adjustment) > 0.1f && !UIManager.Instance.IsMouseOnUI()) {
//         //         fov -= adjustment;
//         //         fov = Mathf.Clamp(fov, _minFov, _maxFov);
//         //
//         //         mainCamera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(mainCamera, adjustment));
//         //         //if (!Mathf.Approximately(previousCameraFOV, fov)) {
//         //         //    previousCameraFOV = fov;
//         //         //    mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
//         //         //} else {
//         //         //    mainCamera.orthographicSize = fov;
//         //         //}
//         //         // CalculateCameraBounds(mainCamera);
//         //         Messenger.Broadcast(Signals.ZOOM_WORLD_MAP_CAMERA, mainCamera);
//         //     }
//         // }
//     }
//     #endregion
//
//     #region Listeners
//     private void OnMenuOpened(InfoUIBase openedBase) { }
//     private void OnMenuClosed(InfoUIBase openedBase) { }
//     private void OnInnerMapOpened(Region location) {
//         // _mainCamera.cullingMask = 0;
//         _raycaster.enabled = false;
//         gameObject.SetActive(false);
//         SetCameraControlState(false);
//     }
//     private void OnInnerMapClosed(Region location) {
//         // _mainCamera.cullingMask = defaultMask;
//         _raycaster.enabled = true;
//         gameObject.SetActive(true);
//         SetCameraControlState(true);
//     }
//     #endregion
//
//     #region Camera Control
//     private void SetCameraControlState(bool state) {
//         cameraControlEnabled = state;
//     }
//     #endregion
// }
