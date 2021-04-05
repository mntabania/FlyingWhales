using UnityEngine;
using UnityEngine.Events;

public class UIModelGenericPopupMenu : PopupMenuBase {
    [SerializeField] private UnityEvent closeAction;
    public override void Close() {
        closeAction?.Invoke();
    }
}