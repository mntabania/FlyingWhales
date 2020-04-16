using UnityEngine;

public class BaseCameraMove : MonoBehaviour{

    [Header("Panning")]
    [SerializeField] private float cameraPanSpeed = 50f;
    
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
    #endregion
}
