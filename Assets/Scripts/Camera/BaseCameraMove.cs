using Inner_Maps;
using Ruinarch;
using UnityEngine;

public class BaseCameraMove : MonoBehaviour{

    [Header("Panning")]
    [SerializeField] private float cameraPanSpeed = 50f;
    
    [Header("Dragging")]
    private float dragThreshold = 0.1f;
    private float currDragTime;
    private Vector3 dragOrigin;
    public bool isDragging = false;
    private bool startedOnUI = false;
    private bool hasReachedThreshold = false;
    private Vector3 originMousePos;
    
    #region Movement
    protected void ArrowKeysMovement() {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            if (!UIManager.Instance.IsConsoleShowing()) { 
                float zAxisValue = Input.GetAxis("Vertical");
                transform.Translate(new Vector3(0f, zAxisValue * Time.deltaTime * cameraPanSpeed, 0f));
                Messenger.Broadcast(Signals.CAMERA_MOVED_BY_PLAYER);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            if (!UIManager.Instance.IsConsoleShowing()) {
                float xAxisValue = Input.GetAxis("Horizontal");
                transform.Translate(new Vector3(xAxisValue * Time.deltaTime * cameraPanSpeed, 0f, 0f));
                Messenger.Broadcast(Signals.CAMERA_MOVED_BY_PLAYER);
            }
        }
    }
    protected void Dragging(Camera targetCamera) {
        if (startedOnUI) {
            if (!Input.GetMouseButton(2)) {
                ResetDragValues();
            }
            return;
        }
        if (!isDragging) {
            if (Input.GetMouseButtonDown(2)) {
                if (UIManager.Instance.IsMouseOnUI() || InnerMapManager.Instance.currentlyHoveredPoi != null) { //if the dragging started on UI, a tileobject or a character, do not allow drag
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
                        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Drag_Clicked);
                        isDragging = true;
                    }
                }
            }
            
        }

        if (isDragging) {
            Vector3 difference = (targetCamera.ScreenToWorldPoint(Input.mousePosition))- targetCamera.transform.position;
            targetCamera.transform.position = dragOrigin-difference;
            Messenger.Broadcast(Signals.CAMERA_MOVED_BY_PLAYER);
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
        //CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        currDragTime = 0f;
        isDragging = false;
        startedOnUI = false;
        hasReachedThreshold = false;
    }
    #endregion
}
