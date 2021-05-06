using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkTextItemUI : PooledObject, BookmarkableEventDispatcher.IListener {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private Button btnMain;
    [SerializeField] private Button btnRemove;
    [SerializeField] private HoverHandler hoverHandler;
    [SerializeField] private UIHoverPosition hoverPosition;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    private System.Action _onHoverOverAction;
    private System.Action _onHoverOutAction;
    
    public void SetBookmark(IBookmarkable p_bookmarkable) {
        SetBookmarkItemText(p_bookmarkable.bookmarkName);
        p_bookmarkable.bookmarkEventDispatcher.Subscribe(this, p_bookmarkable);
        btnMain.onClick.AddListener(p_bookmarkable.OnSelectBookmark);
        btnRemove.onClick.AddListener(() => OnClickRemoveBookmark(p_bookmarkable));
        btnRemove.gameObject.SetActive(p_bookmarkable.bookmarkType == BOOKMARK_TYPE.Text_With_Cancel || p_bookmarkable.bookmarkType == BOOKMARK_TYPE.Special);
        _onHoverOverAction = () => p_bookmarkable.OnHoverOverBookmarkItem(hoverPosition);
        _onHoverOutAction = p_bookmarkable.OnHoverOutBookmarkItem;
        hoverHandler.AddOnHoverOverAction(OnHoverOverBookmark);
        hoverHandler.AddOnHoverOutAction(OnHoverOutBookmark);
    }
    public override void Reset() {
        base.Reset();
        _onHoverOverAction = null;
        _onHoverOutAction = null;
        hoverHandler.RemoveOnHoverOutAction(OnHoverOverBookmark);
        hoverHandler.RemoveOnHoverOverAction(OnHoverOutBookmark);
        btnMain.onClick.RemoveAllListeners();
        btnRemove.onClick.RemoveAllListeners();
    }
    private void OnClickRemoveBookmark(IBookmarkable p_bookmarkable) {
        p_bookmarkable.RemoveBookmark();
        if (p_bookmarkable is IStoredTarget storedTarget) {
            PlayerManager.Instance.player.storedTargetsComponent.Remove(storedTarget);    
        }
    }
    public void OnBookmarkRemoved(IBookmarkable p_bookmarkable) {
        System.Action hoverOutAction = _onHoverOutAction;
        RectTransform rect = null;
        RectTransform parentOfParent = null;
        if (transform.parent is RectTransform rectTransform) {
            rect = rectTransform;
            parentOfParent = transform.parent.parent.parent as RectTransform;
        }
        p_bookmarkable.bookmarkEventDispatcher.Unsubscribe(this, p_bookmarkable);
        ObjectPoolManager.Instance.DestroyObject(this);
        hoverOutAction?.Invoke();
        if (rect) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);    
        }
        if (parentOfParent) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentOfParent);
        }
    }
    public void OnBookmarkChangedName(IBookmarkable p_bookmarkable) {
        SetBookmarkItemText(p_bookmarkable.bookmarkName);
    }

    private void SetBookmarkItemText(string p_text) {
        lblName.text = p_text;
        envelopContent.Execute();
    }
    private void OnHoverOverBookmark() {
        _onHoverOverAction?.Invoke();
    }
    private void OnHoverOutBookmark() {
        _onHoverOutAction?.Invoke();
    }
}
