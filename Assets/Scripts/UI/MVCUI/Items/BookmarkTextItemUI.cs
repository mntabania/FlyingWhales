using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkTextItemUI : PooledObject, BookmarkableEventDispatcher.IListener {
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private Button btnMain;
    [SerializeField] private Button btnRemove;
    
    public void SetBookmark(IBookmarkable p_bookmarkable) {
        lblName.text = p_bookmarkable.bookmarkName;
        p_bookmarkable.bookmarkEventDispatcher.Subscribe(this);
        btnMain.onClick.AddListener(p_bookmarkable.OnSelectBookmark);
        btnRemove.onClick.AddListener(() => OnClickRemoveBookmark(p_bookmarkable));
        btnRemove.gameObject.SetActive(p_bookmarkable.bookmarkType == BOOKMARK_TYPE.Text_With_Cancel);
    }
    public override void Reset() {
        base.Reset();
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
        p_bookmarkable.bookmarkEventDispatcher.Unsubscribe(this);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
}
