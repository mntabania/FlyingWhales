public static class ControlsSignals {
    /// <summary>
    /// Parameters: Camera, float amount
    /// </summary>
    public static string CAMERA_ZOOM_CHANGED = "OnCameraZoomChanged";
    public static string CAMERA_MOVED_BY_PLAYER = "CameraMovedByPlayer";
    /// <summary>
    /// Parameters: ISelectable clickedObject
    /// </summary>
    public static string SELECTABLE_LEFT_CLICKED = "SelectableLeftClicked";
    /// <summary>
    /// Parameters: KeyCode (Pressed Key)
    /// </summary>
    public static string KEY_DOWN = "OnKeyDown";
    public static string KEY_DOWN_EMPTY_SPACE = "OnKeyDownEmptySpace";
    public static string ZOOM_WORLD_MAP_CAMERA = "OnZoomWorldMapCamera";
    /// <summary>
    /// Parameters: KeyCode (Pressed Key)
    /// </summary>
    public static string KEY_UP = "OnKeyUp";
    public static string LEFT_SHIFT_DOWN = "OnLeftShiftPressed";
    public static string LEFT_SHIFT_UP = "OnLeftShiftUp";
}