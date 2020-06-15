using UnityEngine;
using System.Collections;
using Inner_Maps;
using Ruinarch;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using DG.Tweening;

public class InnerMapCameraMove : BaseCameraMove {

	public static InnerMapCameraMove Instance;
    
	[SerializeField] private float zoomSensitivity;
    [SerializeField] private float _zoomSpeed = 5f;
    [FormerlySerializedAs("areaMapsCamera")] public Camera innerMapsCamera;
    [SerializeField] private bool allowZoom = true;

    [Header("Shaking")]
    [SerializeField] private RFX4_CameraShake cameraShake;

    private float previousCameraFOV;

    [SerializeField] private bool cameraControlEnabled = false;
    [SerializeField] private float xSeeLimit;

    public Tweener innerMapCameraShakeMeteorTween { get; private set; }

    #region getters/setters
    public float currentFOV => innerMapsCamera.orthographicSize;
    public float maxFOV => _maxFov;
    public float minFOV => _minFov;
    #endregion

    private void Awake(){
		Instance = this;
	}
    private void Update() {
        if (!cameraControlEnabled) {
            return;
        }
        ArrowKeysMovement();
        Dragging(innerMapsCamera);
        Edging();
        Zooming();
        Targeting(innerMapsCamera);
        ConstrainCameraBounds(innerMapsCamera);
    }

    public override void Initialize() {
        base.Initialize();
        gameObject.SetActive(false);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
        
    }

    #region Listeners
    private void OnInnerMapOpened(Region location) {
        gameObject.SetActive(true);
        SetCameraControlState(true);
        SetCameraBordersForMap(location.innerMap);
        ConstrainCameraBounds(innerMapsCamera);
        innerMapsCamera.depth = 2;
    }
    private void OnInnerMapClosed(Region location) {
        gameObject.SetActive(false);
        SetCameraControlState(false);
        innerMapsCamera.depth = 0;
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
    public void CenterCameraOnTile(HexTile tile) {
        MoveCamera(tile.worldPosition);
    }
    private void Zooming() {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (allowZoom && screenRect.Contains(Input.mousePosition)) {
            //camera scrolling code
            float fov = innerMapsCamera.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (zoomSensitivity);
            if (adjustment != 0f && !UIManager.Instance.IsMouseOnUI()) {
                //Debug.Log(adjustment);
                fov -= adjustment;
                //fov = Mathf.Round(fov * 100f) / 100f;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);

                innerMapsCamera.DOOrthoSize(fov, 0.5f).OnUpdate(() => OnZoom(innerMapsCamera, adjustment));

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
    private void SetCameraBordersForMap(InnerTileMap map) {
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
        Vector3 viewPos = innerMapsCamera.WorldToViewportPoint(gridTile.centeredWorldLocation);
        return viewPos.x >= 0 && viewPos.x <= xSeeLimit && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z >= 0;
    }
    #endregion

    #region Camera Control
    public void SetCameraControlState(bool state) {
        cameraControlEnabled = state;
    }
    public void ShakeCamera() {
        cameraShake.PlayShake();
    }
    #endregion

    #region Meteor
    public void MeteorShake() {
        if (!DOTween.IsTweening(innerMapsCamera)) {
            innerMapCameraShakeMeteorTween = innerMapsCamera.DOShakeRotation(0.8f, new Vector3(8f, 8f, 0f), 35, fadeOut: false);
            innerMapCameraShakeMeteorTween.OnComplete(OnTweenComplete);
        } 
        //else {
            //if(innerMapCameraShakeMeteorTween != null) {
            //    innerMapCameraShakeMeteorTween.ChangeEndValue(new Vector3(8f, 8f, 0f), 0.8f);
            //}
        //}
    }
    private void OnTweenComplete() {
        //InnerMapCameraMove.Instance.innerMapsCamera.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));
        innerMapsCamera.transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);
        innerMapCameraShakeMeteorTween = null;
    }
    #endregion
}
