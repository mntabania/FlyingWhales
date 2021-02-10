﻿using Inner_Maps;
using Ruinarch;
using Settings;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public abstract class BaseCameraMove : BaseMonoBehaviour{

    public Camera camera;

    [Header("Bounds")]
    private const float MIN_Z = -10f;
    private const float MAX_Z = -10f;
    [SerializeField] protected float MIN_X;
    [SerializeField] protected float MAX_X;
    [SerializeField] protected float MIN_Y;
    [SerializeField] protected float MAX_Y;
    [SerializeField] protected float _minFov;
    [SerializeField] protected float _maxFov;
    
    [Header("Panning")]
    [SerializeField] private float cameraPanSpeed = 50f;
    
    [Header("Dragging")]
    private float dragThreshold = 0.05f;
    private float currDragTime;
    private Vector3 dragOrigin;
    public bool isDragging = false;
    private bool startedOnUI = false;
    private bool hasReachedThreshold = false;
    private Vector3 originMousePos;
    
    [Header("Edging")]
    [SerializeField] private int edgeBoundary = 30;
    [SerializeField] private float edgingSpeed = 30f;
    [SerializeField] private bool allowEdgePanning;

    [Header("Targeting")]
    [SerializeField] private float dampTime = 0.2f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _targetPos;
    [SerializeField] private bool isUsingVectorTarget;

    [Header("Threat")]
    [SerializeField] protected ThreatParticleEffect threatEffect;


    public Transform lastCenteredTarget { get; private set; }
    protected bool isMovementDisabled;

    public bool allowSmoothCameraFollow;
    public float smoothFollowSpeed;

    public Transform target {
        get => _target;
        protected set {
            _target = value;
            if (_target == null) {
                Messenger.RemoveListener<GameObject>(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
            } else {
                Messenger.AddListener<GameObject>(ObjectPoolSignals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
            }
        }
    }

    #region Initialization
    public virtual void Initialize() {
        AllowEdgePanning(SettingsManager.Instance.settings.useEdgePanning);
        Messenger.AddListener<bool>(SettingsSignals.EDGE_PANNING_TOGGLED, AllowEdgePanning);
    }
    #endregion

    #region Listeners
    private void AllowEdgePanning(bool state) {
        allowEdgePanning = state;
    }
    #endregion
    
    #region Movement
    protected bool CanMoveCamera() {
        if (SaveManager.Instance.saveCurrentProgressManager.isSaving) {
            //Do not allow hotkeys while saving
            return false;
        }
        if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive()) {
            //Do not allow hotkeys while loading
            return false;
        }
        if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing()) {
            return false;
        }
        if (UIManager.Instance != null && UIManager.Instance.IsObjectPickerOpen()) {
            return false;
        }
        return !isMovementDisabled;
    }
    protected void ArrowKeysMovement() {
        if (!CanMoveCamera()) { return; }
        if (InputManager.Instance.HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            if (!UIManager.Instance.IsConsoleShowing()) {
                float yAxisValue = Input.GetAxis("Vertical");
                Vector3 targetPos = new Vector3(0f, yAxisValue * Time.deltaTime * cameraPanSpeed, 0f);
                if (allowSmoothCameraFollow) {
                    transform.DOBlendableMoveBy(targetPos, smoothFollowSpeed);
                } else {
                    transform.Translate(targetPos);
                }
                Messenger.Broadcast(ControlsSignals.CAMERA_MOVED_BY_PLAYER, targetPos);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            if (!UIManager.Instance.IsConsoleShowing()) {
                float xAxisValue = Input.GetAxis("Horizontal");
                Vector3 targetPos = new Vector3(xAxisValue * Time.deltaTime * cameraPanSpeed, 0f, 0f);
                if (allowSmoothCameraFollow) {
                    transform.DOBlendableMoveBy(targetPos, smoothFollowSpeed);
                } else {
                    transform.Translate(targetPos);
                }
                Messenger.Broadcast(ControlsSignals.CAMERA_MOVED_BY_PLAYER, targetPos);
            }
        }
    }
    protected void Dragging(Camera targetCamera) {
        if (!CanMoveCamera()) { return; }
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
                //dragOrigin = Input.mousePosition; //on first press of mouse
            } else if (Input.GetMouseButton(2)) {
                currDragTime += Time.deltaTime; //while the left mouse button is pressed
                if (currDragTime >= dragThreshold) {
                    if (!hasReachedThreshold) {
                        dragOrigin = targetCamera.ScreenToWorldPoint(Input.mousePosition);
                        originMousePos = Input.mousePosition;
                        hasReachedThreshold = true;
                    }
                    if (originMousePos !=  Input.mousePosition) { //check if the mouse has moved position from the origin, only then will it be considered dragging
                        if (PlayerManager.Instance.player == null || !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI) {
                            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);    
                        }
                        isDragging = true;
                    }
                }
            }
            
        }

        if (isDragging) {
            Vector3 difference = (targetCamera.ScreenToWorldPoint(Input.mousePosition))- targetCamera.transform.position;
            targetCamera.transform.position = dragOrigin-difference;
            Messenger.Broadcast(ControlsSignals.CAMERA_MOVED_BY_PLAYER, difference);
            if (Input.GetMouseButtonUp(2)) {
                ResetDragValues();
                if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Drag_Clicked) {
                    InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);    
                }
                
            } else if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Default) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);
            }
        } else {
            if (!Input.GetMouseButton(2)) {
                currDragTime = 0f;
                hasReachedThreshold = false;
            }
        }
    }
    private void ResetDragValues() {
        //CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        currDragTime = 0f;
        isDragging = false;
        startedOnUI = false;
        hasReachedThreshold = false;
    }
    public void DisableMovement() {
        isMovementDisabled = true;
    }
    public void EnableMovement() {
        isMovementDisabled = false;
    }
    #endregion

    #region Edge Panning
    protected void Edging() {
        if (!allowEdgePanning || isDragging) {
            return;
        }
        bool isEdging = false;
        Vector3 newPos = transform.position;
        if (Input.mousePosition.x >= (Screen.width - (edgeBoundary + 5f))) {
            newPos.x += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.x <= 0 + edgeBoundary) {
            newPos.x -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }

        if (Input.mousePosition.y >= Screen.height - edgeBoundary) {
            newPos.y += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.y <= 0 + edgeBoundary) {
            newPos.y -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (isEdging) {
            target = null; //reset target
        }
        if (allowSmoothCameraFollow) {
            Vector3 pos = newPos - transform.position;
            transform.DOBlendableMoveBy(pos, smoothFollowSpeed);
            Messenger.Broadcast(ControlsSignals.CAMERA_MOVED_BY_PLAYER, pos);
        } else {
            transform.position = newPos;
            Messenger.Broadcast(ControlsSignals.CAMERA_MOVED_BY_PLAYER, newPos);
        }
    }
    #endregion

    #region Targeting
    public void CenterCameraOn(GameObject GO) {
        isUsingVectorTarget = false;
        target = GO.transform;
    }
    public void CenterCameraOn(Vector3 pos) {
        _targetPos = pos;
        isUsingVectorTarget = true;
        target = null;
    }
    protected void Targeting(Camera camera) {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || isDragging) { //|| Input.GetMouseButtonDown(0)
            //reset target when player pushes a button to pan the camera
            target = null;
            isUsingVectorTarget = false;
        }

        if (target) {
            var position = target.position;
            var thisPosition = transform.position;
            Vector3 point = camera.WorldToViewportPoint(position);
            Vector3 delta = position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = thisPosition + delta;
            transform.position = Vector3.SmoothDamp(thisPosition, destination, ref velocity, dampTime);
            if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
                lastCenteredTarget = target;
                target = null;
            }
        } else if (isUsingVectorTarget) {
            var position = _targetPos;
            var thisPosition = transform.position;
            Vector3 point = camera.WorldToViewportPoint(position);
            Vector3 delta = position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = thisPosition + delta;
            transform.position = Vector3.SmoothDamp(thisPosition, destination, ref velocity, dampTime);
            if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
                isUsingVectorTarget = false;
            }
        } 
        
        // if (target) { //smooth camera center
        //     var position = target.position;
        //     var thisPosition = transform.position;
        //     Vector3 point = camera.WorldToViewportPoint(position);
        //     Vector3 delta = position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
        //     Vector3 destination = thisPosition + delta;
        //     transform.position = Vector3.SmoothDamp(thisPosition, destination, ref velocity, dampTime);
        //     if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
        //         lastCenteredTarget = target;
        //         target = null;
        //     }
        // }
    }
    private void OnPooledObjectDestroyed(GameObject obj) {
        if (target == obj.transform) {
            target = null;
        }
    }
    #endregion
    
    #region Bounds
    protected void CalculateCameraBounds(Camera camera) {
        if (GridMap.Instance.map == null) {
            return;
        }
        Area topRightTile = GridMap.Instance.map[GridMap.Instance.width - 1, GridMap.Instance.height - 1];
        // Vector3 topRightTilePosition = topRightTile.transform.position;
        
        Bounds newBounds = new Bounds {
            extents = new Vector3(Mathf.Abs(topRightTilePosition.x),
                Mathf.Abs(topRightTilePosition.y), 0f)
        };
        SetCameraBounds(newBounds);
    }
    protected void ConstrainCameraBounds(Camera camera) {
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
        camera.transform.position = new Vector3(
            xCoord,
            yCoord,
            zCoord);
    }
    public bool HasReachedBounds() {
        if ((Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X)) &&
                (Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y))) {
            return true;
        }
        return false;
    }
    public bool HasReachedMinXBounds() {
        if (transform.position.x <= MIN_X) {
            return true;
        }
        return false;
    }
    public bool HasReachedMaxXBounds() {
        if (transform.position.x >= MAX_X) {
            return true;
        }
        return false;
    }
    public bool HasReachedMinYBounds() {
        if (transform.position.y <= MIN_Y) {
            return true;
        }
        return false;
    }
    public bool HasReachedMaxYBounds() {
        if (transform.position.y >= MAX_Y) {
            return true;
        }
        return false;
    }
    public bool HasReachedMapMinXBoundOf(Region region) {
        float regionMinXPos = region.innerMap.transform.position.x;
        float cameraMinXPos = camera.transform.position.x - (camera.orthographicSize * 2);
        if (cameraMinXPos <= regionMinXPos) {
            return true;
        }
        return false;
    }
    public bool HasReachedMapMaxXBoundOf(Region region) {
        float regionMaxXPos = region.innerMap.transform.position.x + region.innerMap.width;
        float cameraMaxXPos = camera.transform.position.x + (camera.orthographicSize * 2);
        if (cameraMaxXPos >= regionMaxXPos) {
            return true;
        }
        return false;
    }
    public bool HasReachedMapMinYBoundOf(Region region) {
        float regionMinYPos = region.innerMap.transform.position.y;
        float cameraMinYPos = camera.transform.position.y - camera.orthographicSize;
        if (cameraMinYPos <= regionMinYPos) {
            return true;
        }
        return false;
    }
    public bool HasReachedMapMaxYBoundOf(Region region) {
        float regionMaxYPos = region.innerMap.transform.position.y + region.innerMap.height;
        float cameraMaxYPos = camera.transform.position.y + camera.orthographicSize;
        if (cameraMaxYPos >= regionMaxYPos) {
            return true;
        }
        return false;
    }
    private void SetCameraBounds(Bounds bounds) {
        float halfOfHexagon = 256f / 100f;
        // MIN_X = bounds.min.x + horzExtent - (halfOfHexagon * 1.5f);
        // MAX_X = bounds.max.x - horzExtent + (halfOfHexagon * 1.5f); //removed -1 because of UI
        // MIN_Y = bounds.min.y + vertExtent - (halfOfHexagon * 1.5f);
        // MAX_Y = bounds.max.y - vertExtent + (halfOfHexagon * 1.5f);
        MIN_X = bounds.min.x + (halfOfHexagon * 2f);
        MAX_X = bounds.max.x - (halfOfHexagon * 2f);
        MIN_Y = bounds.min.y + halfOfHexagon;
        MAX_Y = bounds.max.y - halfOfHexagon;
    }
    #endregion

    #region Zoom
    protected void OnZoom(Camera camera, float amount) {
        if(threatEffect != null) {
            threatEffect.OnZoomCamera(camera);
        }
        Messenger.Broadcast(ControlsSignals.CAMERA_ZOOM_CHANGED, camera, amount);
    }
    #endregion
    
}
