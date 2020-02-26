using UnityEngine;

public abstract class PopupMenuBase : MonoBehaviour {

    public bool isShowing { get; private set; }
    private void OnEnable() {
        isShowing = true;
        Messenger.Broadcast(Signals.POPUP_MENU_OPENED, this);
    }
    private void OnDisable() {
        isShowing = false;
        Messenger.Broadcast(Signals.POPUP_MENU_CLOSED, this);
    }

    public virtual void Open() {
        gameObject.SetActive(true);
    }
    public virtual void Close() {
        gameObject.SetActive(false);
    }
}
