using System;
using Ruinarch;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour {

	public static CameraMove Instance;
    [SerializeField] private Physics2DRaycaster _raycaster;

    [Header("Bounds")]
    private const float MIN_Z = -10f;
    private const float MAX_Z = -10f;
    [SerializeField] internal float MIN_X;
    [SerializeField] internal float MAX_X;
    [SerializeField] internal float MIN_Y;
    [SerializeField] internal float MAX_Y;
    [SerializeField] private float _minFov;
    [SerializeField] private float _maxFov;
    
    [Header("Targeting")]
    [SerializeField] private float dampTime = 0.2f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
	[SerializeField] private Transform target;

    [Header("Zooming")]
    [SerializeField] private bool allowZoom = true;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float sensitivity;
    
    [Header("Panning")]
    [SerializeField] private float cameraPanSpeed = 50f;
    
    [Header("Dragging")]
    [SerializeField] private float dragThreshold = 0.13f;
    [SerializeField] private float currDragTime;
    [SerializeField] private Vector3 dragOrigin;
    public bool isDragging;

    [Header("Edging")]
    [SerializeField] private int edgeBoundary = 30;
    [SerializeField] private float edgingSpeed = 30f;
    [SerializeField] private bool allowEdgePanning;
    
    

    //private properties
    private float previousCameraFOV;
    private bool cameraControlEnabled = true;
    private int defaultMask;
    private Camera _mainCamera;
    private Transform _mainCameraTransform;

    private void Awake(){
		Instance = this;
        _mainCamera = Camera.main;
        _mainCameraTransform = _mainCamera.transform;
        defaultMask = _mainCamera.cullingMask;
    }
    private void Update() {
        if (!cameraControlEnabled) {
            return;
        }
        ArrowKeysMovement();
        Dragging();
        Edging();
        Zooming();
        Targeting();
        ConstrainCameraBounds();
    }
    private void OnDestroy() {
        RemoveListeners();
    }

    public void Initialize() {
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<ILocation>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<ILocation>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
    }

    private void RemoveListeners() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<ILocation>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.RemoveListener<ILocation>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
    }

    #region Utilities
    public void ToggleMainCameraLayer(string layerName) {
        int cullingMask = _mainCamera.cullingMask;
        cullingMask ^= 1 << LayerMask.NameToLayer(layerName);
        _mainCamera.cullingMask = cullingMask;
        defaultMask = cullingMask;
    }
    #endregion

    #region Positioning
    private void OnGameLoaded() {
        Vector3 initialPos = new Vector3(-2.35f, -1.02f, -10f);
        this.transform.position = initialPos;
        _raycaster.enabled = true;
        CalculateCameraBounds();
    }
    public void CenterCameraOn(GameObject GO) {
        target = GO.transform;
    }
    private void ArrowKeysMovement() {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            //&& (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() == null)
            if (!UIManager.Instance.IsConsoleShowing()) { 
                float zAxisValue = Input.GetAxis("Vertical");
                // iTween.MoveUpdate(_mainCamera.gameObject, iTween.Hash("y", _mainCamera.transform.position.y + zAxisValue, "time", 0.1f));
                transform.Translate(new Vector3(0f, zAxisValue * Time.deltaTime * cameraPanSpeed, 0f));
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            if (!UIManager.Instance.IsConsoleShowing()) {
                float xAxisValue = Input.GetAxis("Horizontal");
                // iTween.MoveUpdate(_mainCamera.gameObject, iTween.Hash("x", _mainCamera.transform.position.x + xAxisValue, "time", 0.1f));
                transform.Translate(new Vector3(xAxisValue * Time.deltaTime * cameraPanSpeed, 0f, 0f));
            }
        }
    }
    private void Zooming() {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (allowZoom && screenRect.Contains(Input.mousePosition)) {
            //camera scrolling code
            float fov = _mainCamera.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (sensitivity);
            if (Math.Abs(adjustment) > 0.1f && !UIManager.Instance.IsMouseOnUI()) {
                fov -= adjustment;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);

                if (!Mathf.Approximately(previousCameraFOV, fov)) {
                    previousCameraFOV = fov;
                    _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
                } else {
                    _mainCamera.orthographicSize = fov;
                }
                CalculateCameraBounds();
            }
        }
    }
    private void Targeting() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || isDragging || Input.GetMouseButtonDown(0)) {
            //reset target when player pushes a button to pan the camera
            target = null;
        }

        if (target) { //smooth camera center
            var position = target.position;
            var thisPosition = transform.position;
            Vector3 point = _mainCamera.WorldToViewportPoint(position);
            Vector3 delta = position - _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = thisPosition + delta;
            transform.position = Vector3.SmoothDamp(thisPosition, destination, ref velocity, dampTime);
            if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
                target = null;
            }
        }
    }
    private bool startedOnUI;
    private bool hasReachedThreshold;
    private Vector3 originMousePos;
    private void Dragging() {
        if (startedOnUI) {
            if (!Input.GetMouseButton(2)) {
                ResetDragValues();
            }
            return;
        }
        if (!isDragging) {
            if (Input.GetMouseButtonDown(2)) {
                if (UIManager.Instance.IsMouseOnUI()) { //if the dragging started on UI, a tileobject or a character, do not allow drag
                    startedOnUI = true;
                    return;
                }
            } else if (Input.GetMouseButton(2)) {
                currDragTime += Time.deltaTime; //while the left mouse button is pressed
                if (currDragTime >= dragThreshold) {
                    if (!hasReachedThreshold) {
                        dragOrigin = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                        originMousePos = Input.mousePosition;
                        hasReachedThreshold = true;
                    }
                    if (originMousePos != Input.mousePosition) { //check if the mouse has moved position from the origin, only then will it be considered dragging
                        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);
                        isDragging = true;
                    }
                }
            }

        }


        if (isDragging) {
            Vector3 difference = (_mainCamera.ScreenToWorldPoint(Input.mousePosition)) - _mainCameraTransform.position;
            _mainCameraTransform.position = dragOrigin - difference;
            if (Input.GetMouseButtonUp(2)) {
                ResetDragValues();
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
            }
        } else {
            if (!Input.GetMouseButton(2)) {
                currDragTime = 0f;
                hasReachedThreshold = false;
            }
        }
    }
    private void ResetDragValues() {
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        currDragTime = 0f;
        isDragging = false;
        startedOnUI = false;
        hasReachedThreshold = false;
    }
    private void Edging() {
        if (!allowEdgePanning || isDragging) {
            return;
        }
        bool isEdging = false;
        Vector3 newPos = transform.position;
        if (Input.mousePosition.x > Screen.width - edgeBoundary) {
            newPos.x += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.x < 0 + edgeBoundary) {
            newPos.x -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }

        if (Input.mousePosition.y > Screen.height - edgeBoundary) {
            newPos.y += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.y < 0 + edgeBoundary) {
            newPos.y -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (isEdging) {
            target = null; //reset target
        }
        transform.position = newPos;
    }
    public void AllowEdgePanning(bool state) {
        allowEdgePanning = state;
    }
    #endregion

    #region Bounds
    public void CalculateCameraBounds() {
        if (GridMap.Instance.map == null) {
            return;
        }
        HexTile rightMostTile = GridMap.Instance.map[GridMap.Instance.width - 1, GridMap.Instance.height / 2];
        HexTile topMostTile = GridMap.Instance.map[GridMap.Instance.width/2, GridMap.Instance.height - 1];

        float vertExtent = _mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        Bounds newBounds = new Bounds {
            extents = new Vector3(Mathf.Abs(rightMostTile.transform.position.x),
                Mathf.Abs(topMostTile.transform.position.y), 0f)
        };
        SetCameraBounds(newBounds, horzExtent, vertExtent);
    }
    private void ConstrainCameraBounds() {
        float xLowerBound = MIN_X;
        float xUpperBound = MAX_X;
        float yLowerBound = MIN_Y;
        float yUpperBound = MAX_Y;
        if (MAX_X < MIN_X) {
            xLowerBound = MAX_X;
            xUpperBound = MIN_X;
        }
        if (MAX_Y < MIN_Y) {
            yLowerBound = MAX_Y;
            yUpperBound = MIN_Y;
        }
        Vector3 thisPos = transform.position;
        float xCoord = Mathf.Clamp(thisPos.x, xLowerBound, xUpperBound);
        float yCoord = Mathf.Clamp(thisPos.y, yLowerBound, yUpperBound);
        float zCoord = Mathf.Clamp(thisPos.z, MIN_Z, MAX_Z);
        _mainCameraTransform.position = new Vector3(
            xCoord,
            yCoord,
            zCoord);
    }
    private bool HasReachedBounds() {
        if ((Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X)) &&
                (Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y))) {
            return true;
        }
        return false;
    }
    private void SetCameraBounds(Bounds bounds, float horzExtent, float vertExtent) {
        float halfOfHexagon = 256f / 100f;
        // MIN_X = bounds.min.x + horzExtent - (halfOfHexagon * ((float)borderCount));
        // MAX_X = bounds.max.x - horzExtent + (halfOfHexagon * (borderCount)); //removed -1 because of UI
        // MIN_Y = bounds.min.y + vertExtent - (halfOfHexagon * ((float)borderCount - 2));
        // MAX_Y = bounds.max.y - vertExtent + (halfOfHexagon * (borderCount - 2));
        MIN_X = bounds.min.x + horzExtent - (halfOfHexagon);
        MAX_X = bounds.max.x - horzExtent + (halfOfHexagon); //removed -1 because of UI
        MIN_Y = bounds.min.y + vertExtent - (halfOfHexagon * 1.5f);
        MAX_Y = bounds.max.y - vertExtent + (halfOfHexagon * 1.5f);
    }
    #endregion

    #region Listeners
    private void OnMenuOpened(InfoUIBase openedBase) { }
    private void OnMenuClosed(InfoUIBase openedBase) { }
    private void OnInnerMapOpened(ILocation location) {
        // _mainCamera.cullingMask = 0;
        _raycaster.enabled = false;
        gameObject.SetActive(false);
        SetCameraControlState(false);
    }
    private void OnInnerMapClosed(ILocation location) {
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
